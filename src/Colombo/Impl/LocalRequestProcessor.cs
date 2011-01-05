using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Core.Logging;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace Colombo.Impl
{
    /// <summary>
    /// Default implementation for processing requests locally.
    /// </summary>
    public class LocalRequestProcessor : IRequestProcessor
    {
        private ILogger logger = NullLogger.Instance;
        public ILogger Logger
        {
            get { return logger; }
            set { logger = value; }
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

        public bool CanSend(BaseRequest request)
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
                string message = "An exception occured inside one or several request handlers";
                Logger.Error(message, ex);
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

        private IColomboHandleInvocation BuildHandleInvocationChain()
        {
            var requestHandlerInvocation = new RequestHandlerHandleInvocation(requestHandlerFactory);
            requestHandlerInvocation.Logger = Logger;
            IColomboHandleInvocation currentInvocation = requestHandlerInvocation;
            return currentInvocation;
        }
    }
}
