using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Colombo
{
    public interface INotificationHandleInterceptor : IColomboInterceptor
    {
        void Intercept(IColomboNotificationHandleInvocation invocation);
    }
}
