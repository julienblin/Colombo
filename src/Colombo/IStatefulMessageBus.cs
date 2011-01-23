using System.Diagnostics.Contracts;

namespace Colombo
{
    /// <summary>
    /// A stateful message bus is a <see cref="IMessageBus"/> that tracks its state and complement the base <see cref="IMessageBus"/>
    /// with additional functionality such as the ability to automatically batch sends and to limit the number of sends it allows.
    /// </summary>
    [ContractClass(typeof(Contracts.StatefulMessageBusContract))]
    public interface IStatefulMessageBus : IMessageBus
    {
        /// <summary>
        /// Return a promise of response - that is a proxy that when accessed the first time, it will send.
        /// This allow the batching of several FutureSend together.
        /// </summary>
        TResponse FutureSend<TResponse>(SideEffectFreeRequest<TResponse> request)
            where TResponse : Response, new();

        /// <summary>
        /// The number of time this <see cref="IStatefulMessageBus"/> has already sent.
        /// </summary>
        int NumberOfSend { get; }

        /// <summary>
        /// The maximum allowed number of send that this <see cref="IStatefulMessageBus"/> will allow.
        /// After this quota, every attempt to send will result in a <see cref="ColomboException"/>.
        /// </summary>
        int MaxAllowedNumberOfSend { get; set; }
    }
}
