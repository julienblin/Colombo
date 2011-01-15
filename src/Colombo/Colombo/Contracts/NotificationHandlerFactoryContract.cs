using System;
using System.Diagnostics.Contracts;

namespace Colombo.Contracts
{
#pragma warning disable 1591 // docs
    [ContractClassFor(typeof(INotificationHandlerFactory))]
    public abstract class NotificationHandlerFactoryContract : INotificationHandlerFactory
    {
        public bool CanCreateNotificationHandlerFor(Notification notification)
        {
            Contract.Requires<ArgumentNullException>(notification != null, "notification");
            throw new NotImplementedException();
        }

        public INotificationHandler[] CreateNotificationHandlersFor(Notification notification)
        {
            Contract.Requires<ArgumentNullException>(notification != null, "notification");
            throw new NotImplementedException();
        }

        public void DisposeNotificationHandler(INotificationHandler notificationHandler)
        {
            Contract.Requires<ArgumentNullException>(notificationHandler != null, "notificationHandler");
            throw new NotImplementedException();
        }
    }
#pragma warning restore 1591
}
