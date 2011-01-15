using System.Diagnostics.Contracts;

namespace Colombo
{
    [ContractClass(typeof(Contracts.GenericSideEffectFreeRequestHandler<,>))]
    public interface ISideEffectFreeRequestHandler<in TRequest, out TResponse> : IRequestHandler
        where TResponse : Response, new()
        where TRequest : SideEffectFreeRequest<TResponse>, new()
    {
        TResponse Handle(TRequest request);
    }
}
