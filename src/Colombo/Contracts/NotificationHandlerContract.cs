using System;
using System.Diagnostics.Contracts;

namespace Colombo.Contracts
{
#pragma warning disable 1591 // docs
    [ContractClassFor(typeof(INotificationHandler))]
    public abstract class NotificationHandlerContract : INotificationHandler
    {
        public void Handle(Notification notification)
        {
            Contract.Requires<ArgumentNullException>(notification != null, "notification");
            throw new NotImplementedException();
        }
    }
#pragma warning restore 1591
}
