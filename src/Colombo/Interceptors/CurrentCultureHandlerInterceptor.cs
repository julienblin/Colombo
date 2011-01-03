using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Globalization;

namespace Colombo.Interceptors
{
    public class CurrentCultureHandlerInterceptor : IRequestHandlerInterceptor
    {
        public void Intercept(IColomboInvocation invocation)
        {
            if (invocation == null) throw new ArgumentNullException("invocation");
            Contract.EndContractBlock();

            if (!string.IsNullOrWhiteSpace(invocation.Request.CallContext.Culture))
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(invocation.Request.CallContext.Culture);
                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(invocation.Request.CallContext.Culture);
            }
            
            invocation.Proceed();
        }

        public int InterceptionPriority
        {
            get { return InterceptorPrority.Medium; }
        }
    }
}
