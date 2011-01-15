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
        protected TNotification Notification { get; private set; }

        public void Handle(Notification notification)
        {
            if (notification == null) throw new ArgumentNullException("notification");
            Contract.EndContractBlock();

            Handle((TNotification)notification);
        }

        public void Handle(TNotification notification)
        {
            Notification = notification;
            Handle();
        }

        protected abstract void Handle();

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
