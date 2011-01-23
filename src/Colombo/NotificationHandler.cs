using System;
using System.Diagnostics.Contracts;

namespace Colombo
{
    /// <summary>
    /// Use this base class to create notification handlers.
    /// </summary>
    public abstract class NotificationHandler<TNotification> : INotificationHandler<TNotification>
        where TNotification : Notification
    {
        /// <summary>
        /// Incoming notification.
        /// </summary>
        protected TNotification Notification { get; private set; }

        /// <summary>
        /// Handles the notification.
        /// </summary>
        public void Handle(Notification notification)
        {
            if (notification == null) throw new ArgumentNullException("notification");
            Contract.EndContractBlock();

            Handle((TNotification)notification);
        }

        /// <summary>
        /// Handles the notification.
        /// </summary>
        public void Handle(TNotification notification)
        {
            Notification = notification;
            Handle();
        }

        /// <summary>
        /// Handles the notification.
        /// </summary>
        protected abstract void Handle();

        /// <summary>
        /// Create a new request to be used inside this notification handler.
        /// The CorrelationGuid and the Context are copied.
        /// </summary>
        protected TRequest CreateRequest<TRequest>()
            where TRequest : BaseRequest, new()
        {
            var result = new TRequest
                             {
                                 CorrelationGuid = Notification.CorrelationGuid,
                                 Context = Notification.Context
                             };
            return result;
        }

        /// <summary>
        /// Create a new notification to be used inside this notification handler.
        /// The CorrelationGuid and the Context are copied.
        /// </summary>
        protected TNewNotification CreateNotification<TNewNotification>()
            where TNewNotification : Notification, new()
        {
            var result = new TNewNotification
                             {
                                 CorrelationGuid = Notification.CorrelationGuid,
                                 Context = Notification.Context
                             };
            return result;
        }
    }
}
