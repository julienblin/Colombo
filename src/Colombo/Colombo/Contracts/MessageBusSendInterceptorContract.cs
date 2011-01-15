using System;
using System.Diagnostics.Contracts;

namespace Colombo.Contracts
{
    [ContractClassFor(typeof(IMessageBusSendInterceptor))]
    public abstract class MessageBusSendInterceptorContract : IMessageBusSendInterceptor
    {
        public void Intercept(IColomboSendInvocation nextInvocation)
        {
            Contract.Requires<ArgumentNullException>(nextInvocation != null, "nextInvocation");
            throw new NotImplementedException();
        }

        public int InterceptionPriority
        {
            get { throw new NotImplementedException(); }
        }
    }
}
