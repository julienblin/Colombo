using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Transactions;

namespace Colombo.Interceptors
{
    /// <summary>
    /// <see cref="IRequestHandlerHandleInterceptor"/> that surrounds Handle operation with a <see cref="TransactionScope"/>.
    /// </summary>
    public class TransactionScopeHandleInterceptor : IRequestHandlerHandleInterceptor
    {
        public void Intercept(IColomboHandleInvocation nextInvocation)
        {
            if (nextInvocation == null) throw new ArgumentNullException("invocation");
            Contract.EndContractBlock();

            using (var tx = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                nextInvocation.Proceed();
                tx.Complete();
            }
        }

        public int InterceptionPriority
        {
            get { return InterceptorPrority.High; }
        }
    }
}
