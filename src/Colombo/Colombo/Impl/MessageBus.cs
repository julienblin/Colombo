using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Core.Logging;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using System.Threading;
using Colombo.Impl.Async;
using Colombo.Impl.Send;
using Colombo.Impl.Notify;

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
                        Logger.Info("No interceptor has been registered for sending requests.");
                    else
                        Logger.InfoFormat("Sending requests with the following interceptors: {0}", string.Join(", ", messageBusSendInterceptors.Select(x => x.GetType().Name)));
                }
            }
        }

        private IMessageBusNotifyInterceptor[] messageBusNotifyInterceptors = new IMessageBusNotifyInterceptor[0];
        /// <summary>
        /// The list of <see cref="IMessageBusSendInterceptor"/> to use.
        /// </summary>
        public IMessageBusNotifyInterceptor[] MessageBusNotifyInterceptors
        {
            get { return messageBusNotifyInterceptors; }
            set
            {
                if (value == null) throw new ArgumentNullException("MessageBusNotifyInterceptors");
                Contract.EndContractBlock();

                messageBusNotifyInterceptors = value.OrderBy(x => x.InterceptionPriority).ToArray();
                if (Logger.IsInfoEnabled)
                {
                    if (messageBusNotifyInterceptors.Length == 0)
                        Logger.Info("No interceptor has been registered for sending notifications.");
                    else
                        Logger.InfoFormat("Sending notifications with the following interceptors: {0}", string.Join(", ", messageBusNotifyInterceptors.Select(x => x.GetType().Name)));
                }
            }
        }

        private readonly IRequestProcessor[] requestProcessors;
        private readonly INotificationProcessor[] notificationProcessors;

        public MessageBus(IRequestProcessor[] requestProcessors)
            : this(requestProcessors, null)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="requestProcessors">List of <see cref="IRequestProcessor"/> that could process requests.</param>
        /// <param name="notificationProcessors">List of <see cref="INotificationProcessor"/> that could process notifications.</param>
        public MessageBus(IRequestProcessor[] requestProcessors, INotificationProcessor[] notificationProcessors)
        {
            if ((requestProcessors == null) || (requestProcessors.Length == 0)) throw new ArgumentException("requestProcessors should have at least one IRequestProcessor.");
            Contract.EndContractBlock();

            this.requestProcessors = requestProcessors;
            this.notificationProcessors = notificationProcessors;
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

            var response = InternalSendAsync<TResponse>(request);
            if (response == null)
                LogAndThrowError("Internal error: response should not be null.");

            Contract.Assume(response != null);
            return response;
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

        public TResponse Send<TRequest, TResponse>(Action<TRequest> action)
            where TRequest : SideEffectFreeRequest<TResponse>, new()
            where TResponse : Response, new()
        {
            if (action == null) throw new ArgumentNullException("action");
            Contract.EndContractBlock();

            var request = new TRequest();
            action(request);
            return Send(request);
        }

        public IAsyncCallback<TResponse> SendAsync<TResponse>(SideEffectFreeRequest<TResponse> request)
            where TResponse : Response, new()
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();

            var response = InternalSendAsync<TResponse>(request);
            if (response == null)
                LogAndThrowError("Internal error: response should not be null.");

            Contract.Assume(response != null);
            return response;
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

        public void Notify(Notification notification, params Notification[] notifications)
        {
            if ((notificationProcessors == null) || (notificationProcessors.Length == 0))
                throw new ColomboException("Unable to Notify: no INotificationProcessor has been registered.");

            if (notification == null) throw new ArgumentNullException("notification");
            Contract.EndContractBlock();

            var finalNotifications = new List<Notification>();
            finalNotifications.Add(notification);
            if(notifications != null)
                finalNotifications.AddRange(notifications);

            Logger.DebugFormat("Sending notifications {0}...", string.Join(", ", finalNotifications.Select(x => x.ToString())));

            var topInvocation = BuildNotifyInvocationChain();
            topInvocation.Notifications = finalNotifications;
            topInvocation.Proceed();
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

        protected virtual IAsyncCallback<TResponse> InternalSendAsync<TResponse>(BaseRequest request)
            where TResponse : Response, new()
        {
            var asyncCallback = new AsyncCallback<TResponse>();
            Task.Factory.StartNew((c) =>
            {
                try
                {
                    var responsesGroup = InternalSend(new List<BaseRequest> { (BaseRequest)request });
                    ((AsyncCallback<TResponse>)c).ResponseArrived((TResponse)responsesGroup[request]);
                }
                catch (Exception ex)
                {
                    ((AsyncCallback<TResponse>)c).ExceptionArrived(ex);
                }
            },
            asyncCallback);

            Contract.Assert(asyncCallback != null);
            return asyncCallback;
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

        private IColomboNotifyInvocation BuildNotifyInvocationChain()
        {
            var notificationProcessorsInvocation = new NotificationProcessorNotifyInvocation(notificationProcessors);
            notificationProcessorsInvocation.Logger = Logger;
            IColomboNotifyInvocation currentInvocation = notificationProcessorsInvocation;

            foreach (var interceptor in MessageBusNotifyInterceptors.Reverse())
            {
                if (interceptor != null)
                    currentInvocation = new MessageBusNotifyInterceptorInvocation(interceptor, currentInvocation);
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
