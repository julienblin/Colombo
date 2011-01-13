using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.DynamicProxy;
using System.Diagnostics.Contracts;
using System.Reflection;

namespace Colombo.Impl.Async
{
    public class StatefulReponseInterceptor : IInterceptor
    {
        private static readonly MethodInfo Exception_InternalPreserveStackTrace =
    typeof(Exception).GetMethod("InternalPreserveStackTrace", BindingFlags.Instance | BindingFlags.NonPublic);

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
                try
                {
                    invocation.ReturnValue = invocation.Method.Invoke(internalResponse, invocation.Arguments);
                }
                catch (TargetInvocationException tie)
                {
                    // Propagate the inner exception so that the proxy throws the same exception as
                    // the real object would 
                    Exception_InternalPreserveStackTrace.Invoke(tie.InnerException, new Object[] { });
                    throw tie.InnerException;
                }
            }
            else
            {
                invocation.Proceed();
            }
        }
    }
}
