using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Colombo
{
    [ContractClass(typeof(Contracts.MessageBusSendInterceptorContract))]
    public interface IMessageBusSendInterceptor : IInterceptor
    {
        Response BeforeSend(BaseRequest request);
        void AfterMessageProcessorSend(BaseRequest request, Response response);
    }
}
