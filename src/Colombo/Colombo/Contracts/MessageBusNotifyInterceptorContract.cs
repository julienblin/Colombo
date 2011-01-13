using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Colombo.Contracts
{
    [ContractClassFor(typeof(IMessageBusNotifyInterceptor))]
    public abstract class MessageBusNotifyInterceptorContract : IMessageBusNotifyInterceptor
    {
        public void Intercept(IColomboNotifyInvocation invocation)
        {
            Contract.Requires<ArgumentNullException>(invocation != null, "invocation");
            throw new NotImplementedException();
        }

        public int InterceptionPriority
        {
            get { throw new NotImplementedException(); }
        }
    }
}
