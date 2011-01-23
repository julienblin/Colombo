using System;
using System.Diagnostics.Contracts;
using System.Transactions;

namespace Colombo.Interceptors
{
    /// <summary>
    /// <see cref="INotificationHandleInterceptor"/> that surrounds Handle operations for notifications with a <see cref="TransactionScope"/>.
    /// </summary>
    public class TransactionScopeNotificationHandleInterceptor : INotificationHandleInterceptor
    {
        /// <summary>
        /// Surrounds the following invocations inside a <see cref="TransactionScope"/>
        /// </summary>
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

        /// <summary>
        /// High.
        /// </summary>
        public int InterceptionPriority
        {
            get { return InterceptionPrority.High; }
        }
    }
}
