using System;
using System.Diagnostics.Contracts;
using System.Linq;
using Colombo.Impl.Notify;
using Colombo.Impl.Send;

namespace Colombo.Impl
{
    /// <summary>
    /// Default implementaion of <see cref="IMessageBus"/>.
    /// </summary>
    public class MessageBus : BaseMessageBus
    {
        private readonly IRequestProcessor[] requestProcessors;
        private readonly INotificationProcessor[] notificationProcessors;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="requestProcessors">List of <see cref="IRequestProcessor"/> that could process requests.</param>
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

        /// <summary>
        /// Return a invocation chain for the Send operation.
        /// </summary>
        protected override IColomboSendInvocation BuildSendInvocationChain()
        {
            Contract.Assume(requestProcessors != null);
            Contract.Assume(requestProcessors.Length != 0);
            Contract.Assume(MessageBusSendInterceptors != null);

            var requestProcessorInvocation = new RequestProcessorSendInvocation(requestProcessors) { Logger = Logger };
            IColomboSendInvocation currentInvocation = requestProcessorInvocation;

            foreach (var interceptor in MessageBusSendInterceptors.Reverse())
            {
                if (interceptor != null)
                    currentInvocation = new MessageBusSendInterceptorInvocation(interceptor, currentInvocation);
            }

            return currentInvocation;
        }

        /// <summary>
        /// Return a invocation chain for the Notify operation.
        /// </summary>
        protected override IColomboNotifyInvocation BuildNotifyInvocationChain()
        {
            if ((notificationProcessors == null) || (notificationProcessors.Length == 0))
                throw new ColomboException("Unable to Notify: no INotificationProcessor has been registered.");

            var notificationProcessorsInvocation = new NotificationProcessorNotifyInvocation(notificationProcessors);
            IColomboNotifyInvocation currentInvocation = notificationProcessorsInvocation;

            foreach (var interceptor in MessageBusNotifyInterceptors.Reverse())
            {
                if (interceptor != null)
                    currentInvocation = new MessageBusNotifyInterceptorInvocation(interceptor, currentInvocation);
            }

            return currentInvocation;
        }
    }
}
