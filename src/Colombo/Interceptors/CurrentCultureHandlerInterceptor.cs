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
        public void Intercept(IColomboSingleInvocation invocation)
        {
            if (invocation == null) throw new ArgumentNullException("invocation");
            Contract.EndContractBlock();

            if (invocation.Request.Context.Keys.Contains(CurrentCultureConstant.CultureContextKey))
            {
                if (!string.IsNullOrWhiteSpace(invocation.Request.Context[CurrentCultureConstant.CultureContextKey]))
                {
                    Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(invocation.Request.Context[CurrentCultureConstant.CultureContextKey]);
                    Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(invocation.Request.Context[CurrentCultureConstant.CultureContextKey]);
                }
            }
            else
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            }
            
            invocation.Proceed();
        }

        public int InterceptionPriority
        {
            get { return InterceptorPrority.Medium; }
        }
    }
}
