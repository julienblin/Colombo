using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Colombo.Contracts
{
    [ContractClassFor(typeof(IColomboSingleInterceptor))]
    public abstract class ColomboInterceptorContract : IColomboSingleInterceptor
    {
        void IColomboSingleInterceptor.Intercept(IColomboSingleInvocation invocation)
        {
            Contract.Requires<ArgumentNullException>(invocation != null, "invocation");
        }

        int IColomboInterceptor.InterceptionPriority
        {
            get { return default(int); }
        }
    }
}
