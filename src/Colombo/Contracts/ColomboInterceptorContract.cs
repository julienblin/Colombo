using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Colombo.Contracts
{
    [ContractClassFor(typeof(IColomboInterceptor))]
    public abstract class ColomboInterceptorContract : IColomboInterceptor
    {
        void IColomboInterceptor.Intercept(IColomboInvocation invocation)
        {
            Contract.Requires<ArgumentNullException>(invocation != null, "invocation");
        }

        int IColomboInterceptor.InterceptionPriority
        {
            get { return default(int); }
        }
    }
}
