using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Transactions;
using System.Diagnostics.Contracts;

namespace Colombo.Interceptors
{
    public class TransactionScopeHandlerInterceptor : IRequestHandlerInterceptor
    {
        public void Intercept(IColomboSingleInvocation invocation)
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
