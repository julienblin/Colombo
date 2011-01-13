using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Colombo.Impl.NotificationHandle
{
    public class NotificationHandleInterceptorInvocation : BaseNotificationHandleInvocation
    {
        private readonly INotificationHandleInterceptor interceptor;
        private readonly IColomboNotificationHandleInvocation nextInvocation;

        public NotificationHandleInterceptorInvocation(INotificationHandleInterceptor interceptor, IColomboNotificationHandleInvocation nextInvocation)
        {
            if (interceptor == null) throw new ArgumentNullException("interceptor");
            if (nextInvocation == null) throw new ArgumentNullException("nextInvocation");
            Contract.EndContractBlock();

            this.interceptor = interceptor;
            this.nextInvocation = nextInvocation;
        }

        public override void Proceed()
        {
            nextInvocation.Notification = Notification;
            interceptor.Intercept(nextInvocation);
        }
    }
}
