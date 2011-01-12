using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Colombo
{
    [ContractClass(typeof(Contracts.NotificationHandlerFactoryContract))]
    public interface INotificationHandlerFactory
    {
        bool CanCreateNotificationHandlerFor(Notification notification);

        INotificationHandler CreateNotificationHandlerFor(Notification notification);

        void DisposeNotificationHandler(INotificationHandler notificationHandler);
    }
}
