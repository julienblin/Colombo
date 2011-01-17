using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Colombo.TestSupport
{
    public class MessageBusNotifyExpectation<TNotification> : BaseExpectation
        where TNotification : Notification, new()
    {
        public MessageBusNotifyExpectation(IStubMessageBus stubMessageBus)
            : base(stubMessageBus)
        {
        }

        internal override object Execute(object parameter)
        {
            throw new NotImplementedException();
        }

        public override void Verify()
        {
            throw new NotImplementedException();
        }
    }
}
