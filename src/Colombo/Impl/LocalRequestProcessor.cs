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
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using Castle.Core.Logging;
using Colombo.Impl.RequestHandle;

namespace Colombo.Impl
{
    /// <summary>
    /// Default implementation for processing requests locally.
    /// </summary>
    public class LocalRequestProcessor : ILocalRequestProcessor
    {
        private ILogger logger = NullLogger.Instance;
        /// <summary>
        /// Logger.
        /// </summary>
        public ILogger Logger
        {
            get { return logger; }
            set { logger = value; }
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

            Logger.Debug("Parallel processing of the requests with local handlers...");
            var tasks = new List<Task<Response>>();
            var tasksRequestAssociation = new Dictionary<BaseRequest, Task<Response>>();
            foreach (var request in requests)
            {
                var task = Task.Factory.StartNew(req =>
                    {
                        var topInvocation = BuildHandleInvocationChain();
                        topInvocation.Request = (BaseRequest)req;
                        topInvocation.Proceed();
                        return topInvocation.Response;
                    },
                    request
                    );
                tasks.Add(task);
                tasksRequestAssociation[request] = task;
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
                throw new ColomboException(message, ex);
            }

            Logger.Debug("All the request handlers have executed successfully.");

            Logger.Debug("Reconstituing responses...");
            var responses = new ResponsesGroup();

            foreach (var request in requests)
            {
                responses[request] = tasksRequestAssociation[request].Result;
            }

            Contract.Assume(responses.Count == requests.Count);
            return responses;
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
