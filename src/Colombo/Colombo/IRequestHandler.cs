using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Colombo
{
    /// <summary>
    /// Base class for requests handlers.
    /// </summary>
    [ContractClass(typeof(Contracts.RequestHandlerContract))]
    public interface IRequestHandler
    {
        Response Handle(BaseRequest request);

        Type GetRequestType();
        Type GetResponseType();
    }

    [ContractClass(typeof(Contracts.GenericRequestHandlerContract<,>))]
    public interface IRequestHandler<TRequest, TResponse> : IRequestHandler
        where TResponse : Response, new()
        where TRequest : Request<TResponse>, new()
    {
        TResponse Handle(TRequest request);
    }


}
