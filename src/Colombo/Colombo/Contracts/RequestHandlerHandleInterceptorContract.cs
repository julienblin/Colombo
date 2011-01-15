using System;
using System.Diagnostics.Contracts;

namespace Colombo.Contracts
{
#pragma warning disable 1591 // docs
    [ContractClassFor(typeof(IRequestHandlerHandleInterceptor))]
    public abstract class RequestHandlerHandleInterceptorContract : IRequestHandlerHandleInterceptor
    {
        public void Intercept(IColomboRequestHandleInvocation nextInvocation)
        {
            Contract.Requires<ArgumentNullException>(nextInvocation != null, "nextInvocation");
            throw new NotImplementedException();
        }

        public int InterceptionPriority
        {
            get { throw new NotImplementedException(); }
        }
    }
#pragma warning restore 1591
}
