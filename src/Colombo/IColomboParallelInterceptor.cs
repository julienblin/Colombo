using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Colombo
{
    public interface IColomboParallelInterceptor
    {
        void Intercept(IColomboParallelInvocation invocation);
        int InterceptionPriority { get; }
    }
}
