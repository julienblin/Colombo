using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Colombo.Contracts
{
    [ContractClassFor(typeof(IMessageProcessor))]
    public abstract class MessageProcessorContract : IMessageProcessor
    {
        bool IMessageProcessor.CanSend<TResponse>(Request<TResponse> request)
        {
            Contract.Requires<ArgumentNullException>(request != null, "request");
            return default(bool);
        }

        TResponse IMessageProcessor.Send<TResponse>(Request<TResponse> request)
        {
            Contract.Requires<ArgumentNullException>(request != null, "request");
            Contract.Ensures(Contract.Result<TResponse>() != null);
            return default(TResponse);
        }
    }
}
