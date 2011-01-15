using System.Diagnostics.Contracts;

namespace Colombo
{
    [ContractClass(typeof(Contracts.NotificationHandlerContract))]
    public interface INotificationHandler
    {
        void Handle(Notification notification);
    }

    [ContractClass(typeof(Contracts.GenericNotificationHandler<>))]
    public interface INotificationHandler<in TNotification> : INotificationHandler
        where TNotification : Notification
    {
        void Handle(TNotification notification);
    }
}
