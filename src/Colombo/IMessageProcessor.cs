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
        bool CanSend(BaseRequest request);
        Response Send(BaseRequest request);

        Response[] ParallelSend(BaseRequest[] requests);
    }
}
