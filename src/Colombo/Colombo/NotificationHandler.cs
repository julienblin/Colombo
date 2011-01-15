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
        private TNotification notification;
        protected TNotification Notification { get { return notification; } }

        public void Handle(Notification notification)
        {
            if (notification == null) throw new ArgumentNullException("notification");
            Contract.EndContractBlock();

            Handle((TNotification)notification);
        }

        public void Handle(TNotification notification)
        {
            this.notification = notification;
            Handle();
        }

        public abstract void Handle();

        protected TRequest CreateRequest<TRequest>()
            where TRequest : BaseRequest, new()
        {
            var result = new TRequest();
            result.CorrelationGuid = Notification.CorrelationGuid;
            result.Context = Notification.Context;
            return result;
        }

        protected TNewNotification CreateNotification<TNewNotification>()
            where TNewNotification : Notification, new()
        {
            var result = new TNewNotification();
            result.CorrelationGuid = Notification.CorrelationGuid;
            result.Context = Notification.Context;
            return result;
        }
    }
}
