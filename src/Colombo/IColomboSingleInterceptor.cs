using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Colombo
{
    [ContractClass(typeof(Contracts.ColomboSingleInterceptorContract))]
    public interface IColomboSingleInterceptor : IColomboInterceptor
    {
        void Intercept(IColomboSingleInvocation invocation);
    }
}
