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
        Response IMessageBusSendInterceptor.BeforeSend(BaseRequest request)
        {
            Contract.Requires<ArgumentNullException>(request != null, "request");
            return default(Response);
        }

        void IMessageBusSendInterceptor.AfterMessageProcessorSend(BaseRequest request, Response response)
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
