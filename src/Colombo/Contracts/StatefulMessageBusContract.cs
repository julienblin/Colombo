using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Colombo.Contracts
{
    [ContractClassFor(typeof(IStatefulMessageBus))]
    public abstract class StatefulMessageBusContract : IStatefulMessageBus
    {
        public TResponse FutureSend<TResponse>(SideEffectFreeRequest<TResponse> request)
            where TResponse : Response, new()
        {
            Contract.Requires<ArgumentNullException>(request != null, "request");
            Contract.Ensures(Contract.Result<TResponse>() != null);
            throw new NotImplementedException();
        }

        public int NumberOfSend
        {
            get { throw new NotImplementedException(); }
        }

        public int MaxAllowedNumberOfSend
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public TResponse Send<TResponse>(Request<TResponse> request)
            where TResponse : Response, new()
        {
            throw new NotImplementedException();
        }

        public TResponse Send<TResponse>(SideEffectFreeRequest<TResponse> request)
            where TResponse : Response, new()
        {
            throw new NotImplementedException();
        }

        public ResponsesGroup Send(BaseSideEffectFreeRequest request, params BaseSideEffectFreeRequest[] followingRequests)
        {
            throw new NotImplementedException();
        }
    }
}
