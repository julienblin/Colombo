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
            ExpectedNumCalled = 1;
        }

        private TNotification receivedNotification;

        internal override object Execute(object parameter)
        {
            ++NumCalled;
            receivedNotification = (TNotification) parameter;
            return null;
        }

        public MessageBusNotifyExpectation<TNotification> Repeat(int times)
        {
            ExpectedNumCalled = times;
            return this;
        }

        private Action<TNotification> assertion;

        public MessageBusNotifyExpectation<TNotification> Assert(Action<TNotification> action)
        {
            assertion = action;
            return this;
        }

        public override void Verify()
        {
            if (ExpectedNumCalled != NumCalled)
                throw new ColomboExpectationException(string.Format("Expected {0} to notify {1} time(s), actual: {2}", typeof(TNotification), ExpectedNumCalled, NumCalled));

            if((receivedNotification != null) && (assertion != null))
                assertion.Invoke(receivedNotification);
        }
    }
}
