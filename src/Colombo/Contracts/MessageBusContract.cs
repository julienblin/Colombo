using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Colombo.Contracts
{
    [ContractClassFor(typeof(IMessageBus))]
    public abstract class MessageBusContract : IMessageBus
    {
        public TResponse Send<TResponse>(Request<TResponse> request) where TResponse : Response, new()
        {
            Contract.Requires<ArgumentNullException>(request != null, "request");
            Contract.Ensures(Contract.Result<TResponse>() != null);
            throw new NotImplementedException();
        }

        public ResponsesGroup Send(BaseSideEffectFreeRequest request1, BaseSideEffectFreeRequest request2, params BaseSideEffectFreeRequest[] followingRequests)
        {
            Contract.Requires<ArgumentNullException>(request1 != null, "request1");
            Contract.Requires<ArgumentNullException>(request2 != null, "request2");
            Contract.Ensures(Contract.Result<ResponsesGroup>() != null);
            throw new NotImplementedException();
        }
    }
}
