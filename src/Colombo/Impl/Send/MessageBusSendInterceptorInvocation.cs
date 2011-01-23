using System;
using System.Diagnostics.Contracts;

namespace Colombo.Impl.Send
{
    internal class MessageBusSendInterceptorInvocation : BaseSendInvocation
    {
        private readonly IMessageBusSendInterceptor interceptor;
        private readonly IColomboSendInvocation nextInvocation;

        public MessageBusSendInterceptorInvocation(IMessageBusSendInterceptor interceptor, IColomboSendInvocation nextInvocation)
        {
            if (interceptor == null) throw new ArgumentNullException("interceptor");
            if (nextInvocation == null) throw new ArgumentNullException("nextInvocation");
            Contract.EndContractBlock();

            this.interceptor = interceptor;
            this.nextInvocation = nextInvocation;
        }

        public override void Proceed()
        {
            nextInvocation.Requests = Requests;
            interceptor.Intercept(nextInvocation);
            Responses = nextInvocation.Responses;
        }
    }
}
