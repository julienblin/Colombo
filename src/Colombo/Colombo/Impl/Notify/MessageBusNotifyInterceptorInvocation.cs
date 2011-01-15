using System;
using System.Diagnostics.Contracts;

namespace Colombo.Impl.Notify
{
    public class MessageBusNotifyInterceptorInvocation : BaseNotifyInvocation
    {
        private readonly IMessageBusNotifyInterceptor interceptor;
        private readonly IColomboNotifyInvocation nextInvocation;

        public MessageBusNotifyInterceptorInvocation(IMessageBusNotifyInterceptor interceptor, IColomboNotifyInvocation nextInvocation)
        {
            if (interceptor == null) throw new ArgumentNullException("interceptor");
            if (nextInvocation == null) throw new ArgumentNullException("nextInvocation");
            Contract.EndContractBlock();

            this.interceptor = interceptor;
            this.nextInvocation = nextInvocation;
        }

        public override void Proceed()
        {
            nextInvocation.Notifications = Notifications;
            interceptor.Intercept(nextInvocation);
        }
    }
}
