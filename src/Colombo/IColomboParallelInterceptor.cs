using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Colombo
{
    [ContractClass(typeof(Contracts.ColomboParallelInterceptorContract))]
    public interface IColomboParallelInterceptor : IColomboInterceptor
    {
        void Intercept(IColomboParallelInvocation invocation);
    }
}
