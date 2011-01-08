using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Core.Logging;
using System.Diagnostics.Contracts;

namespace Colombo.Impl
{
    /// <summary>
    /// Default implementaion of <see cref="IMessageBus"/>.
    /// </summary>
    public class MessageBus : IMessageBus
    {
        private ILogger logger = NullLogger.Instance;
        public ILogger Logger
        {
            get { return logger; }
            set { logger = value; }
        }

        private IMessageBusSendInterceptor[] messageBusSendInterceptors = new IMessageBusSendInterceptor[0];
        /// <summary>
        /// The list of <see cref="IMessageBusSendInterceptor"/> to use.
        /// </summary>
        public IMessageBusSendInterceptor[] MessageBusSendInterceptors
        {
            get { return messageBusSendInterceptors; }
            set
            {
                if (value == null) throw new ArgumentNullException("MessageBusSendInterceptors");
                Contract.EndContractBlock();

                messageBusSendInterceptors = value.OrderBy(x => x.InterceptionPriority).ToArray();
                if (Logger.IsInfoEnabled)
                {
                    if (messageBusSendInterceptors.Length == 0)
                        Logger.Info("No interceptor has been registered for sending.");
                    else
                        Logger.InfoFormat("Sending with the following interceptors: {0}", string.Join(", ", messageBusSendInterceptors.Select(x => x.GetType().Name)));
                }
            }
        }

        private readonly IRequestProcessor[] requestProcessors;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="requestProcessors">List of <see cref="IRequestProcessor"/> that could process the request.</param>
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

        public IAsyncCallback<TResponse> SendAsync<TResponse>(Request<TResponse> request)
            where TResponse : Response, new()
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();
            throw new NotImplementedException();
        }

        public TResponse Send<TResponse>(SideEffectFreeRequest<TResponse> request)
            where TResponse : Response, new()
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();

            var responses = InternalSend(new List<BaseRequest> { request });
            Contract.Assume(responses != null);
            Contract.Assume(responses.Count == 1);

            var typedResponse = responses[request] as TResponse;
            if (typedResponse == null)
                LogAndThrowError("Internal error: InternalSend returned null or incorrect response type: expected {0}, actual {1}.", typeof(TResponse), responses[request].GetType());

            return typedResponse;
        }

        public IAsyncCallback<TResponse> SendAsync<TResponse>(SideEffectFreeRequest<TResponse> request)
            where TResponse : Response, new()
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();
            throw new NotImplementedException();
        }

        public ResponsesGroup Send(BaseSideEffectFreeRequest request, params BaseSideEffectFreeRequest[] followingRequests)
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();

            var listRequests = new List<BaseRequest> { request };
            if(followingRequests != null)
                listRequests.AddRange(followingRequests);
            var responsesGroup = InternalSend(listRequests);
            
            Contract.Assume(responsesGroup != null);
            return responsesGroup;
        }

        protected virtual ResponsesGroup InternalSend(IList<BaseRequest> requests)
        {
            var topInvocation = BuildSendInvocationChain();
            topInvocation.Requests = requests;
            topInvocation.Proceed();

            if (topInvocation.Responses == null)
                throw new ColomboException("Internal error: responses should not be null");

            return topInvocation.Responses;
        }

        private IColomboSendInvocation BuildSendInvocationChain()
        {
            Contract.Assume(requestProcessors != null);
            Contract.Assume(requestProcessors.Length != 0);
            Contract.Assume(MessageBusSendInterceptors != null);

            var requestProcessorInvocation = new RequestProcessorSendInvocation(requestProcessors);
            requestProcessorInvocation.Logger = Logger;
            IColomboSendInvocation currentInvocation = requestProcessorInvocation;

            foreach (var interceptor in MessageBusSendInterceptors.Reverse())
            {
                if (interceptor != null)
                    currentInvocation = new MessageBusSendInterceptorInvocation(interceptor, currentInvocation);
            }

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
