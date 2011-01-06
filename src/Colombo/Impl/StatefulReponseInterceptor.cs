using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.DynamicProxy;
using System.Diagnostics.Contracts;

namespace Colombo.Impl
{
    public class StatefulReponseInterceptor : IInterceptor
    {
        private readonly StatefulMessageBus statefulMessageBus;
        private readonly BaseSideEffectFreeRequest request;

        public StatefulReponseInterceptor(StatefulMessageBus statefulMessageBus, BaseSideEffectFreeRequest request)
        {
            if (statefulMessageBus == null) throw new ArgumentNullException("statefulMessageBus");
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();

            this.statefulMessageBus = statefulMessageBus;
            this.request = request;
        }

        Response internalResponse;

        public void Intercept(IInvocation invocation)
        {
            if (internalResponse == null)
            {
                internalResponse = statefulMessageBus.GetResponseForPendingRequest(request);
            }

            if (internalResponse != null)
            {
                invocation.ReturnValue = invocation.Method.Invoke(internalResponse, invocation.Arguments);
            }
            else
            {
                invocation.Proceed();
            }
        }
    }
}
