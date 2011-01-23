using System.Diagnostics.Contracts;

namespace Colombo
{
    /// <summary>
    /// Represents a component that can create and dispose <see cref="INotificationHandler"/>.
    /// </summary>
    [ContractClass(typeof(Contracts.NotificationHandlerFactoryContract))]
    public interface INotificationHandlerFactory
    {
        /// <summary>
        /// Return true if it can create a <see cref="INotificationHandler"/> that handles the <paramref name="notification"/>, false otherwise.
        /// </summary>
        bool CanCreateNotificationHandlerFor(Notification notification);

        /// <summary>
        /// Create the <see cref="INotificationHandler">notification handlers</see> that can handle the <paramref name="notification"/>.
        /// </summary>
        INotificationHandler[] CreateNotificationHandlersFor(Notification notification);

        /// <summary>
        /// Dispose the <paramref name="notificationHandler"/>.
        /// </summary>
        void DisposeNotificationHandler(INotificationHandler notificationHandler);
    }
}
