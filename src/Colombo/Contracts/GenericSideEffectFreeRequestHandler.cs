using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Colombo.Contracts
{
    [ContractClassFor(typeof(ISideEffectFreeRequestHandler<,>))]
    public abstract class GenericSideEffectFreeRequestHandler<TRequest, TResponse> : ISideEffectFreeRequestHandler<TRequest, TResponse>
        where TResponse : Response, new()
        where TRequest : SideEffectFreeRequest<TResponse>, new()
    {
        public TResponse Handle(TRequest request)
        {
            Contract.Requires<ArgumentNullException>(request != null, "request");
            Contract.Ensures(Contract.Result<TResponse>() != null);
            throw new NotImplementedException();
        }

        public Response Handle(BaseRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
