using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Colombo
{
    [ContractClass(typeof(Contracts.RequestHandlerContract))]
    public interface IRequestHandler
    {
        Response Handle(BaseRequest request);
    }

    public interface IRequestHandler<TRequest, TResponse> : IRequestHandler
        where TResponse : Response, new()
        where TRequest : Request<TResponse>, new()
    {
        TResponse Handle(TRequest request);
    }
}
