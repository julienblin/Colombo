using System.Diagnostics.Contracts;

namespace Colombo
{
    /// <summary>
    /// Interface used to defined request handlers for side effect-free requests.
    /// You may prefer to use the abstract SideEffectFreeRequestHandler class instead.
    /// </summary>
    [ContractClass(typeof(Contracts.GenericSideEffectFreeRequestHandler<,>))]
    public interface ISideEffectFreeRequestHandler<in TRequest, out TResponse> : IRequestHandler
        where TResponse : Response, new()
        where TRequest : SideEffectFreeRequest<TResponse>, new()
    {
        /// <summary>
        /// Handles the request.
        /// </summary>
        TResponse Handle(TRequest request);
    }
}
