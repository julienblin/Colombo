using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Colombo.Impl.Notify;

namespace Colombo.TestSupport
{
    internal class StubNotifyInvocation : BaseNotifyInvocation
    {
        private readonly IStubMessageBus stubMessageBus;

        public StubNotifyInvocation(IStubMessageBus stubMessageBus)
        {
            this.stubMessageBus = stubMessageBus;
        }

        public override void Proceed()
        {
            foreach (var invoc in BuildHandleInvocationChains(Notifications))
                Task.Factory.StartNew(i => ((IColomboNotificationHandleInvocation)i).Proceed(), invoc);
        }

        private IEnumerable<IColomboNotificationHandleInvocation> BuildHandleInvocationChains(IEnumerable<Notification> notifications)
        {
            foreach (var notification in notifications)
            {
                IColomboNotificationHandleInvocation currentInvocation = new StubNotificationHandleInvocation(this.stubMessageBus);
                currentInvocation.Notification = notification;
                yield return currentInvocation;
            }
        }
    }
}
