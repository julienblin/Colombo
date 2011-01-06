using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Colombo
{
    [ContractClass(typeof(Contracts.GenericSideEffectFreeRequestHandler<,>))]
    public interface ISideEffectFreeRequestHandler<TRequest, TResponse> : IRequestHandler
        where TResponse : Response, new()
        where TRequest : SideEffectFreeRequest<TResponse>, new()
    {
        TResponse Handle(TRequest request);
    }
}
