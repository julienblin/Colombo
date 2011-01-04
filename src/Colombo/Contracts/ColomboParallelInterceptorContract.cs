using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Colombo.Contracts
{
    [ContractClassFor(typeof(IColomboParallelInterceptor))]
    public abstract class ColomboParallelInterceptorContract : IColomboParallelInterceptor
    {
        void IColomboParallelInterceptor.Intercept(IColomboParallelInvocation invocation)
        {
            Contract.Requires<ArgumentNullException>(invocation != null, "invocation");
        }

        int IColomboInterceptor.InterceptionPriority
        {
            get { return default(int); }
        }
    }
}
