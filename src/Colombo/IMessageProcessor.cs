using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Colombo
{
    [ContractClass(typeof(Contracts.MessageProcessorContract))]
    public interface IMessageProcessor
    {
        bool CanSend<TResponse>(Request<TResponse> request) where TResponse : Response, new();
        TResponse Send<TResponse>(Request<TResponse> request) where TResponse : Response, new();
    }
}
