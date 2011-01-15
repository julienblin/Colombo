using System.Diagnostics.Contracts;

namespace Colombo
{
    [ContractClass(typeof(Contracts.NotificationHandlerFactoryContract))]
    public interface INotificationHandlerFactory
    {
        bool CanCreateNotificationHandlerFor(Notification notification);

        INotificationHandler[] CreateNotificationHandlersFor(Notification notification);

        void DisposeNotificationHandler(INotificationHandler notificationHandler);
    }
}
