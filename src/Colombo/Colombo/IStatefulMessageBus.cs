using System.Diagnostics.Contracts;

namespace Colombo
{
    [ContractClass(typeof(Contracts.StatefulMessageBusContract))]
    public interface IStatefulMessageBus : IMessageBus
    {
        TResponse FutureSend<TResponse>(SideEffectFreeRequest<TResponse> request)
            where TResponse : Response, new();

        int NumberOfSend { get; }

        int MaxAllowedNumberOfSend { get; set; }
    }
}
