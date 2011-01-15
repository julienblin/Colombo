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
        /// <summary>
        /// Handles the request.
        /// </summary>
        Response Handle(BaseRequest request);

        /// <summary>
        /// Get the type of request that this request handler handles.
        /// </summary>
        /// <returns></returns>
        Type GetRequestType();

        /// <summary>
        /// Get the type of response that this request handler produces.
        /// </summary>
        /// <returns></returns>
        Type GetResponseType();
    }

    /// <summary>
    /// Interface used to defined request handlers for requests.
    /// You may prefer to use the abstract RequestHandler class instead.
    /// </summary>
    [ContractClass(typeof(Contracts.GenericRequestHandlerContract<,>))]
    public interface IRequestHandler<in TRequest, out TResponse> : IRequestHandler
        where TResponse : Response, new()
        where TRequest : Request<TResponse>, new()
    {
        /// <summary>
        /// Handles the request.
        /// </summary>
        TResponse Handle(TRequest request);
    }


}
