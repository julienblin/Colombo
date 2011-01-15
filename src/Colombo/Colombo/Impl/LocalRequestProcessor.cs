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

        public bool CanProcess(BaseRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();

            return requestHandlerFactory.CanCreateRequestHandlerFor(request);
        }

        public ResponsesGroup Process(IList<BaseRequest> requests)
        {
            if (requests == null) throw new ArgumentNullException("requests");
            Contract.EndContractBlock();

            Logger.Debug("Parallel processing of the requests with local handlers...");
            var tasks = new List<Task<Response>>();
            var tasksRequestAssociation = new Dictionary<BaseRequest, Task<Response>>();
            foreach (var request in requests)
            {
                var task = Task.Factory.StartNew<Response>((req) =>
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
                var message = "An exception occured inside one or several request handlers";
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

            var requestHandlerInvocation = new RequestHandlerHandleInvocation(requestHandlerFactory);
            requestHandlerInvocation.Logger = Logger;
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
