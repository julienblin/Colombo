using System;
using System.Diagnostics.Contracts;

namespace Colombo.Contracts
{
    [ContractClassFor(typeof(INotificationHandler))]
    public abstract class NotificationHandlerContract : INotificationHandler
    {
        public void Handle(Notification notification)
        {
            Contract.Requires<ArgumentNullException>(notification != null, "notification");
            throw new NotImplementedException();
        }
    }
}
