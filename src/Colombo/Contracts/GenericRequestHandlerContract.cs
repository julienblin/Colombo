using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Colombo.Contracts
{
    [ContractClassFor(typeof(IRequestHandler<,>))]
    public abstract class GenericRequestHandlerContract<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
        where TResponse : Response, new()
        where TRequest : Request<TResponse>, new()
    {
        TResponse IRequestHandler<TRequest, TResponse>.Handle(TRequest request)
        {
            Contract.Requires<ArgumentNullException>(request != null, "request");
            Contract.Ensures(Contract.Result<TResponse>() != null);
            return default(TResponse);
        }

        Response IRequestHandler.Handle(BaseRequest request)
        {
            return default(Response);
        }
    }
}
