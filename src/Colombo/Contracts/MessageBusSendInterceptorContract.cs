using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Colombo.Contracts
{
    [ContractClassFor(typeof(IMessageBusSendInterceptor))]
    public abstract class MessageBusSendInterceptorContract : IMessageBusSendInterceptor
    {
        TResponse IMessageBusSendInterceptor.BeforeSend<TResponse>(Request<TResponse> request)
        {
            Contract.Requires<ArgumentNullException>(request != null, "request");
            return default(TResponse);
        }

        void IMessageBusSendInterceptor.AfterMessageProcessorSend<TResponse>(Request<TResponse> request, Response response)
        {
            Contract.Requires<ArgumentNullException>(request != null, "request");
            Contract.Requires<ArgumentNullException>(response != null, "response");
        }

        int IInterceptor.InterceptionPriority
        {
            get { return default(int); }
        }
    }
}
