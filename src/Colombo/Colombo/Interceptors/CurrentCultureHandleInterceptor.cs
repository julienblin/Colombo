using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Globalization;

namespace Colombo.Interceptors
{
    /// <summary>
    /// <see cref="IRequestHandlerHandleInterceptor"/> that uses a specific key in Context to set the CurrentThread.CurrentCulture and CurrentUICulture.
    /// </summary>
    public class CurrentCultureHandleInterceptor : IRequestHandlerHandleInterceptor
    {
        public void Intercept(IColomboHandleInvocation nextInvocation)
        {
            if (nextInvocation == null) throw new ArgumentNullException("nextInvocation");
            Contract.EndContractBlock();

            if (nextInvocation.Request.Context.Keys.Contains(CurrentCultureConstant.CultureContextKey))
            {
                if (!string.IsNullOrWhiteSpace(nextInvocation.Request.Context[CurrentCultureConstant.CultureContextKey]))
                {
                    Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(nextInvocation.Request.Context[CurrentCultureConstant.CultureContextKey]);
                    Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(nextInvocation.Request.Context[CurrentCultureConstant.CultureContextKey]);
                }
            }
            else
            {
                Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
            }

            nextInvocation.Proceed();
        }

        public int InterceptionPriority
        {
            get { return InterceptorPrority.Medium; }
        }
    }
}
