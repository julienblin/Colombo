#region License
// The MIT License
// 
// Copyright (c) 2011 Julien Blin, julien.blin@gmail.com
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Castle.Core.Logging;
using Colombo.Impl.RequestHandle;

namespace Colombo.Impl
{
    /// <summary>
    /// Default implementation for processing requests locally.
    /// </summary>
    public class LocalRequestProcessor : ILocalRequestProcessor, IMetaContextKeysManager
    {
        /// <summary>
        /// Constant used for the value associated with the <see cref="MetaContextKeys.EndpointAddressUri"/> when handled locally.
        /// </summary>
        public const string LocalMetaContextKeyEndpointAddressUri = @"local";

        private ILogger logger = NullLogger.Instance;
        /// <summary>
        /// Logger.
        /// </summary>
        public ILogger Logger
        {
            get { return logger; }
            set { logger = value; }
        }

        private IColomboStatCollector statCollector = NullStatCollector.Instance;
        /// <summary>
        /// Stats collector.
        /// </summary>
        public IColomboStatCollector StatCollector
        {
            get { return statCollector; }
            set { statCollector = value; }
        }

        private IRequestHandlerHandleInterceptor[] requestHandlerInterceptors = new IRequestHandlerHandleInterceptor[0];
        /// <summary>
        /// The list of <see cref="IRequestHandlerHandleInterceptor"/> to use.
        /// </summary>
        public IRequestHandlerHandleInterceptor[] RequestHandlerInterceptors
        {
            get { return requestHandlerInterceptors; }
            set
            {
                if (value == null) throw new ArgumentNullException("RequestHandlerInterceptors");
                Contract.EndContractBlock();

                requestHandlerInterceptors = value.OrderBy(x => x.InterceptionPriority).ToArray();
                Logger.InfoFormat("Using the following interceptors: {0}", string.Join(", ", requestHandlerInterceptors.Select(x => x.GetType().Name)));
            }
        }

        private readonly IRequestHandlerFactory requestHandlerFactory;

        private ThreadLocal<IDictionary<int, BaseRequest>> threadLocalRequestReferences = new ThreadLocal<IDictionary<int, BaseRequest>>();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="requestHandlerFactory">The <see cref="IRequestHandlerFactory"/> used to create <see cref="IRequestHandler"/>.</param>
        public LocalRequestProcessor(IRequestHandlerFactory requestHandlerFactory)
        {
            if (requestHandlerFactory == null) throw new ArgumentNullException("requestHandlerFactory");
            Contract.EndContractBlock();

            this.requestHandlerFactory = requestHandlerFactory;
        }

        /// <summary>
        /// Disable the management of <see cref="MetaContextKeys"/>.
        /// </summary>
        public bool DoNotManageMetaContextKeys { get; set; }

        /// <summary>
        /// <c>true</c> if the processor can process the request, <c>false</c> otherwise.
        /// </summary>
        public bool CanProcess(BaseRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();

            return requestHandlerFactory.CanCreateRequestHandlerFor(request);
        }

        /// <summary>
        /// Process the requests.
        /// </summary>
        public ResponsesGroup Process(IList<BaseRequest> requests)
        {
            if (requests == null) throw new ArgumentNullException("requests");
            Contract.EndContractBlock();

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            if (!DoNotManageMetaContextKeys)
            {
                foreach (var request in requests)
                {
                    request.Context[MetaContextKeys.HandlerMachineName] = Environment.MachineName;
                    if (!request.Context.ContainsKey(MetaContextKeys.EndpointAddressUri))
                        request.Context[MetaContextKeys.EndpointAddressUri] = LocalMetaContextKeyEndpointAddressUri;
                }
            }

            Logger.Debug("Parallel processing of the requests with local handlers...");
            var tasks = new List<Task<Response>>();
            var tasksRequestAssociation = new Dictionary<BaseRequest, Task<Response>>();
            foreach (var request in requests)
            {
                var task = new Task<Response>(req =>
                    {
                        if (threadLocalRequestReferences.Value == null)
                            threadLocalRequestReferences.Value = new Dictionary<int, BaseRequest>();

                        threadLocalRequestReferences.Value[Task.CurrentId.Value] = (BaseRequest)req;

                        IColomboRequestHandleInvocation topInvocation;
                        try
                        {
                            topInvocation = BuildHandleInvocationChain();
                            topInvocation.Request = (BaseRequest)req;
                            topInvocation.Proceed();
                        }
                        finally
                        {
                            threadLocalRequestReferences.Value.Remove(Task.CurrentId.Value);
                        }
                        return topInvocation.Response;
                    },
                    request
                    );
                tasks.Add(task);
                tasksRequestAssociation[request] = task;
                task.Start();
            }
            try
            {
                Task.WaitAll(tasks.ToArray());
            }
            catch (AggregateException ex)
            {
                const string message = "An exception occured inside one or several request handlers";
                Logger.Error(message, ex);
                foreach (var innerEx in ex.InnerExceptions)
                    Logger.Error(innerEx.ToString());

                StatCollector.IncrementErrors(requests.Count);
                throw new ColomboException(message, ex);
            }

            Logger.Debug("All the request handlers have executed successfully.");

            Logger.Debug("Reconstituing responses...");
            var responses = new ResponsesGroup();

            foreach (var request in requests)
            {
                responses[request] = tasksRequestAssociation[request].Result;
            }

            stopWatch.Stop();
            StatCollector.IncrementRequestsHandled(requests.Count, stopWatch.Elapsed);

            Contract.Assume(responses.Count == requests.Count);
            return responses;
        }

        /// <summary>
        /// Returns the current request being processed when inside an invocation chain, or <c>null</c> otherwise.
        /// </summary>
        public BaseRequest CurrentRequest
        {
            get
            {
                if ((threadLocalRequestReferences.Value != null) && (Task.CurrentId.HasValue) && (threadLocalRequestReferences.Value.ContainsKey(Task.CurrentId.Value)))
                    return threadLocalRequestReferences.Value[Task.CurrentId.Value];

                return null;
            }
        }

        private IColomboRequestHandleInvocation BuildHandleInvocationChain()
        {
            Contract.Assume(RequestHandlerInterceptors != null);

            var requestHandlerInvocation = new RequestHandlerHandleInvocation(requestHandlerFactory) {Logger = Logger};
            IColomboRequestHandleInvocation currentInvocation = requestHandlerInvocation;
            foreach (var interceptor in RequestHandlerInterceptors.Reverse())
            {
                if (interceptor != null)
                    currentInvocation = new RequestHandlerHandleInterceptorInvocation(interceptor, currentInvocation);
            }
            return currentInvocation;
        }
    }
}
