using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Colombo.Impl.NotificationHandle;

namespace Colombo.TestSupport
{
    internal class StubNotificationHandleInvocation : BaseNotificationHandleInvocation
    {
        private readonly IStubMessageBus stubMessageBus;

        public StubNotificationHandleInvocation(IStubMessageBus stubMessageBus)
        {
            this.stubMessageBus = stubMessageBus;
        }

        public override void Proceed()
        {
            var expectation = stubMessageBus.GetExpectationFor(Notification.GetType());

            if (expectation != null)
            {
                expectation.Execute(Notification);
            }
        }
    }
}
