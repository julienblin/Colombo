using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
