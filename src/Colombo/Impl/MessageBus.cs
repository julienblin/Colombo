using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Core.Logging;
using System.Diagnostics.Contracts;

namespace Colombo.Impl
{
    public class MessageBus : IMessageBus
    {
        private ILogger logger = NullLogger.Instance;
        public ILogger Logger
        {
            get { return logger; }
            set { logger = value; }
        }

        private readonly IRequestProcessor[] requestProcessors;

        public MessageBus(IRequestProcessor[] requestProcessors)
        {
            if ((requestProcessors == null) || (requestProcessors.Length == 0)) throw new ArgumentException("requestProcessors should have at least one IRequestProcessor.");
            Contract.EndContractBlock();

            this.requestProcessors = requestProcessors;
        }

        public TResponse Send<TResponse>(Request<TResponse> request) where TResponse : Response, new()
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();

            var responses = InternalSend(new List<BaseRequest> { request });
            Contract.Assume(responses != null);
            Contract.Assume(responses.Count == 1);

            var typedResponse = responses[request] as TResponse;
            if(typedResponse == null)
                LogAndThrowError("Internal error: InternalSend returned null or incorrect response type: expected {0}, actual {1}.", typeof(TResponse), responses[request].GetType());

            return typedResponse;
        }

        public ResponsesGroup Send(BaseSideEffectFreeRequest request1, BaseSideEffectFreeRequest request2, params BaseSideEffectFreeRequest[] followingRequests)
        {
            var listRequests = new List<BaseRequest> { request1, request2 };
            listRequests.AddRange(followingRequests);
            return InternalSend(listRequests);
        }

        protected virtual ResponsesGroup InternalSend(IList<BaseRequest> requests)
        {
            var topInvocation = BuildSendInvocationChain();
            topInvocation.Requests = requests;
            topInvocation.Proceed();

            return topInvocation.Responses;
        }

        private IColomboSendInvocation BuildSendInvocationChain()
        {
            Contract.Assume(requestProcessors != null);
            Contract.Assume(requestProcessors.Length != 0);

            var requestProcessorInvocation = new RequestProcessorSendInvocation(requestProcessors);
            requestProcessorInvocation.Logger = Logger;
            IColomboSendInvocation currentInvocation = requestProcessorInvocation;
            return currentInvocation;
        }

        protected virtual void LogAndThrowError(string format, params object[] args)
        {
            if (format == null) throw new ArgumentNullException("format");
            if (args == null) throw new ArgumentNullException("args");
            Contract.EndContractBlock();

            var errorMessage = string.Format(format, args);
            Logger.Error(errorMessage);
            throw new ColomboException(errorMessage);
        }
    }
}
