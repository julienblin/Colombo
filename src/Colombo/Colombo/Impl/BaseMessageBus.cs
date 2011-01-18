using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Core.Logging;
using Colombo.Impl.Async;

namespace Colombo.Impl
{
    /// <summary>
    /// Base class for <see cref="IMessageBus"/>.
    /// </summary>
    public abstract class BaseMessageBus : IMessageBus
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
                if (!Logger.IsInfoEnabled) return;

                if (messageBusSendInterceptors.Length == 0)
                    Logger.Info("No interceptor has been registered for sending requests.");
                else
                    Logger.InfoFormat("Sending requests with the following interceptors: {0}", string.Join(", ", messageBusSendInterceptors.Select(x => x.GetType().Name)));
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
                if (!Logger.IsInfoEnabled) return;

                if (messageBusNotifyInterceptors.Length == 0)
                    Logger.Info("No interceptor has been registered for sending notifications.");
                else
                    Logger.InfoFormat("Sending notifications with the following interceptors: {0}", string.Join(", ", messageBusNotifyInterceptors.Select(x => x.GetType().Name)));
            }
        }

        /// <summary>
        /// Send synchronously a request and returns the response.
        /// </summary>
        public virtual TResponse Send<TResponse>(Request<TResponse> request) where TResponse : Response, new()
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

        /// <summary>
        /// Send a request asynchronously. You must register a callback with the result to get the response or the error.
        /// </summary>
        public virtual IAsyncCallback<TResponse> SendAsync<TResponse>(Request<TResponse> request)
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

        /// <summary>
        /// Send synchronously a request and returns the response.
        /// </summary>
        public virtual TResponse Send<TResponse>(SideEffectFreeRequest<TResponse> request)
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

        /// <summary>
        /// Send synchronously a request and returns the response.
        /// </summary>
        public virtual TResponse Send<TRequest, TResponse>(Action<TRequest> action)
            where TRequest : SideEffectFreeRequest<TResponse>, new()
            where TResponse : Response, new()
        {
            if (action == null) throw new ArgumentNullException("action");
            Contract.EndContractBlock();

            var request = new TRequest();
            action(request);
            return Send(request);
        }

        /// <summary>
        /// Send a request asynchronously. You must register a callback with the result to get the response or the error.
        /// </summary>
        public virtual IAsyncCallback<TResponse> SendAsync<TResponse>(SideEffectFreeRequest<TResponse> request)
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

        /// <summary>
        /// Send synchronously, but in parallel, several requests and returns all the responses at once.
        /// Only side effect-free requests can be parallelized.
        /// </summary>
        public virtual ResponsesGroup Send(BaseSideEffectFreeRequest request, params BaseSideEffectFreeRequest[] followingRequests)
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();

            var listRequests = new List<BaseRequest> { request };
            if (followingRequests != null)
                listRequests.AddRange(followingRequests);
            var responsesGroup = InternalSend(listRequests);

            Contract.Assume(responsesGroup != null);
            return responsesGroup;
        }

        /// <summary>
        /// Real sending of the requests. All the other send methods delegates to this one.
        /// Uses <see cref="BuildSendInvocationChain"/>.
        /// </summary>
        protected virtual ResponsesGroup InternalSend(IList<BaseRequest> requests)
        {
            var topInvocation = BuildSendInvocationChain();
            topInvocation.Requests = requests;
            topInvocation.Proceed();

            if (topInvocation.Responses == null)
                throw new ColomboException("Internal error: responses should not be null");

            return topInvocation.Responses;
        }

        /// <summary>
        /// Dispatch notifications
        /// </summary>
        public virtual void Notify(Notification notification, params Notification[] notifications)
        {
            if (notification == null) throw new ArgumentNullException("notification");
            Contract.EndContractBlock();

            var finalNotifications = new List<Notification> { notification };
            if (notifications != null)
                finalNotifications.AddRange(notifications);

            Logger.DebugFormat("Sending notifications {0}...", string.Join(", ", finalNotifications.Select(x => x.ToString())));

            var topInvocation = BuildNotifyInvocationChain();
            topInvocation.Notifications = finalNotifications;
            topInvocation.Proceed();
        }

        /// <summary>
        /// Internal method that is used by Async send operations. Uses <see cref="InternalSend"/> under the cover.
        /// </summary>
        protected virtual IAsyncCallback<TResponse> InternalSendAsync<TResponse>(BaseRequest request)
            where TResponse : Response, new()
        {
            var asyncCallback = new AsyncCallback<TResponse>();
            Task.Factory.StartNew(c =>
            {
                try
                {
                    var responsesGroup = InternalSend(new List<BaseRequest> { request });
                    ((AsyncCallback<TResponse>)c).ResponseArrived((TResponse)responsesGroup[request]);
                }
                catch (Exception ex)
                {
                    ((AsyncCallback<TResponse>)c).ExceptionArrived(ex);
                }
            },
            asyncCallback);

            return asyncCallback;
        }

        /// <summary>
        /// Log an error using the <see cref="Logger"/> and throw a <see cref="ColomboException"/>.
        /// </summary>
        protected virtual void LogAndThrowError(string format, params object[] args)
        {
            if (format == null) throw new ArgumentNullException("format");
            if (args == null) throw new ArgumentNullException("args");
            Contract.EndContractBlock();

            var errorMessage = string.Format(format, args);
            Logger.Error(errorMessage);
            throw new ColomboException(errorMessage);
        }

        /// <summary>
        /// Return a invocation chain for the Send operation.
        /// </summary>
        protected abstract IColomboSendInvocation BuildSendInvocationChain();

        /// <summary>
        /// Return a invocation chain for the Notify operation.
        /// </summary>
        protected abstract IColomboNotifyInvocation BuildNotifyInvocationChain();
    }
}
