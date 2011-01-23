using System;
using System.Diagnostics.Contracts;

namespace Colombo.Contracts
{
#pragma warning disable 1591 // docs
    [ContractClassFor(typeof(INotificationProcessor))]
    public abstract class NotificationProcessorContract : INotificationProcessor
    {
        public void Process(Notification[] notifications)
        {
            Contract.Requires<ArgumentNullException>(notifications != null);
            throw new NotImplementedException();
        }
    }
#pragma warning restore 1591
}
