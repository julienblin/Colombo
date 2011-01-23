using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Colombo.Impl.NotificationHandle;
using Colombo.Impl.Notify;

namespace Colombo.TestSupport
{
    internal class StubNotifyInvocation : BaseNotifyInvocation
    {
        private readonly IStubMessageBus stubMessageBus;
        private readonly INotificationHandleInterceptor[] notificationHandleInterceptors;

        public StubNotifyInvocation(IStubMessageBus stubMessageBus, INotificationHandleInterceptor[] notificationHandleInterceptors)
        {
            this.stubMessageBus = stubMessageBus;
            this.notificationHandleInterceptors = notificationHandleInterceptors;
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
                foreach (var interceptor in notificationHandleInterceptors.Reverse())
                {
                    if (interceptor != null)
                        currentInvocation = new NotificationHandleInterceptorInvocation(interceptor, currentInvocation);
                }
                currentInvocation.Notification = notification;
                yield return currentInvocation;
            }
        }
    }
}
