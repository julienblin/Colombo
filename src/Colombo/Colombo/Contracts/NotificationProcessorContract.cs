using System;
using System.Diagnostics.Contracts;

namespace Colombo.Contracts
{
    [ContractClassFor(typeof(INotificationProcessor))]
    public abstract class NotificationProcessorContract : INotificationProcessor
    {
        public void Process(Notification[] notifications)
        {
            Contract.Requires<ArgumentNullException>(notifications != null);
            throw new NotImplementedException();
        }
    }
}
