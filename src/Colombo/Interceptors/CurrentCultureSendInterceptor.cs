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
    /// <see cref="IRequestHandlerHandleInterceptor"/> that places in a specific key in Context the CurrentThread.CurrentUICulture .
    /// </summary>
    public class CurrentCultureSendInterceptor : IMessageBusSendInterceptor
    {
        public void Intercept(IColomboSendInvocation nextInvocation)
        {
            if (nextInvocation == null) throw new ArgumentNullException("nextInvocation");
            Contract.EndContractBlock();

            foreach (var request in nextInvocation.Requests)
            {
                if ((Thread.CurrentThread.CurrentUICulture != null) && (Thread.CurrentThread.CurrentUICulture != CultureInfo.InvariantCulture))
                    request.Context[CurrentCultureConstant.CultureContextKey] = Thread.CurrentThread.CurrentUICulture.Name;
            }

            nextInvocation.Proceed();
        }

        public int InterceptionPriority
        {
            get { return InterceptorPrority.Medium; }
        }
    }
}
