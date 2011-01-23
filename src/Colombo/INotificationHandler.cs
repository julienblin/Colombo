using System.Diagnostics.Contracts;

namespace Colombo
{
    /// <summary>
    /// Represent a component that handles notifications.
    /// </summary>
    [ContractClass(typeof(Contracts.NotificationHandlerContract))]
    public interface INotificationHandler
    {
        /// <summary>
        /// Handles the notification.
        /// </summary>
        void Handle(Notification notification);
    }

    /// <summary>
    /// Interface used to defined notification handlers.
    /// You may prefer to use the abstract NotificationHandler class instead.
    /// </summary>
    [ContractClass(typeof(Contracts.GenericNotificationHandler<>))]
    public interface INotificationHandler<in TNotification> : INotificationHandler
        where TNotification : Notification
    {
        /// <summary>
        /// Handles the notification.
        /// </summary>
        void Handle(TNotification notification);
    }
}
