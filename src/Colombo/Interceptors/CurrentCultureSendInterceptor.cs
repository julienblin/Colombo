using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics.Contracts;

namespace Colombo.Interceptors
{
    public class CurrentCultureSendInterceptor : IMessageBusSendInterceptor
    {
        public void Intercept(IColomboInvocation invocation)
        {
            if (invocation == null) throw new ArgumentNullException("invocation");
            Contract.EndContractBlock();

            if (Thread.CurrentThread.CurrentUICulture != null)
                invocation.Request.Context[CurrentCultureConstant.CultureContextKey] = Thread.CurrentThread.CurrentUICulture.Name;
            invocation.Proceed();
        }

        public int InterceptionPriority
        {
            get { return InterceptorPrority.Medium; }
        }
    }
}
