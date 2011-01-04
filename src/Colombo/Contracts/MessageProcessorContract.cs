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
        bool IMessageProcessor.CanSend(BaseRequest request)
        {
            Contract.Requires<ArgumentNullException>(request != null, "request");
            return default(bool);
        }

        Response IMessageProcessor.Send(BaseRequest request)
        {
            Contract.Requires<ArgumentNullException>(request != null, "request");
            Contract.Ensures(Contract.Result<Response>() != null);
            return default(Response);
        }

        Response[] IMessageProcessor.ParallelSend(BaseRequest[] requests)
        {
            Contract.Requires<ArgumentNullException>(requests != null, "requests");
            Contract.Ensures(Contract.Result<Response[]>() != null);
            Contract.Ensures(Contract.Result<Response[]>().Length == requests.Length);
            return default(Response[]);
        }
    }
}
