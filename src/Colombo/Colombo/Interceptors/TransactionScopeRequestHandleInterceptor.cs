using System;
using System.Diagnostics.Contracts;
using System.Transactions;

namespace Colombo.Interceptors
{
    /// <summary>
    /// <see cref="IRequestHandlerHandleInterceptor"/> that surrounds Handle operation with a <see cref="TransactionScope"/>.
    /// </summary>
    public class TransactionScopeRequestHandleInterceptor : IRequestHandlerHandleInterceptor
    {
        public void Intercept(IColomboRequestHandleInvocation nextInvocation)
        {
            if (nextInvocation == null) throw new ArgumentNullException("nextInvocation");
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
