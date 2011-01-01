using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Colombo.Contracts
{
    [ContractClassFor(typeof(IMessageBus))]
    public abstract class MessageBusContracts : IMessageBus
    {
        TResponse IMessageBus.Send<TResponse>(Request<TResponse> request)
        {
            Contract.Requires<ArgumentNullException>(request != null, "request");
            Contract.Ensures(Contract.Result<TResponse>() != null);
            return default(TResponse);
        }
    }
}
