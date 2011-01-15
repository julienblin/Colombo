using System;
using System.Diagnostics.Contracts;
using System.Transactions;

namespace Colombo.Interceptors
{
    public class TransactionScopeNotificationHandleInterceptor : INotificationHandleInterceptor
    {
        public void Intercept(IColomboNotificationHandleInvocation invocation)
        {
            if (invocation == null) throw new ArgumentNullException("invocation");
            Contract.EndContractBlock();

            using (var tx = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                invocation.Proceed();
                tx.Complete();
            }
        }

        public int InterceptionPriority
        {
            get { return InterceptorPrority.High; }
        }
    }
}
