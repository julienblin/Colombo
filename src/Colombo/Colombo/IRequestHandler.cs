using System;
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
    public interface IRequestHandler<in TRequest, out TResponse> : IRequestHandler
        where TResponse : Response, new()
        where TRequest : Request<TResponse>, new()
    {
        TResponse Handle(TRequest request);
    }


}
