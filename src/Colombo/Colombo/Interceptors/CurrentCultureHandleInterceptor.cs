using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Threading;

namespace Colombo.Interceptors
{
    /// <summary>
    /// <see cref="IRequestHandlerHandleInterceptor"/> that uses a specific key in Context to set the CurrentThread.CurrentCulture and CurrentUICulture.
    /// </summary>
    public class CurrentCultureHandleInterceptor : IRequestHandlerHandleInterceptor
    {
        public void Intercept(IColomboRequestHandleInvocation nextInvocation)
        {
            if (nextInvocation == null) throw new ArgumentNullException("nextInvocation");
            Contract.EndContractBlock();

            if (nextInvocation.Request.Context.Keys.Contains(CurrentCultureConstant.CultureContextKey))
            {
                var contextValue = nextInvocation.Request.Context[CurrentCultureConstant.CultureContextKey];
                if (!string.IsNullOrWhiteSpace(contextValue))
                {
                    Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo(contextValue);
                    Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(contextValue);
                }
                else
                {
                    Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
                    Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
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
