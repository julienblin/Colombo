using System;
using System.Diagnostics.Contracts;
using System.Transactions;

namespace Colombo.Interceptors
{
    /// <summary>
    /// <see cref="IRequestHandlerHandleInterceptor"/> that surrounds Handle operations for requests with a <see cref="TransactionScope"/>.
    /// </summary>
    public class TransactionScopeRequestHandleInterceptor : IRequestHandlerHandleInterceptor
    {
        /// <summary>
        /// Surrounds the following invocations inside a <see cref="TransactionScope"/>
        /// </summary>
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

        /// <summary>
        /// High.
        /// </summary>
        public int InterceptionPriority
        {
            get { return InterceptionPrority.High; }
        }
    }
}
