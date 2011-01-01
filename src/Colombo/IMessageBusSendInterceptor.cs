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
        TResponse BeforeSend<TResponse>(Request<TResponse> request) where TResponse : Response, new();
        void AfterMessageProcessorSend<TResponse>(Request<TResponse> request, Response response) where TResponse : Response, new();
    }
}
