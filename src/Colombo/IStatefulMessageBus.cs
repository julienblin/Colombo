using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Colombo
{
    [ContractClass(typeof(Contracts.StatefulMessageBusContract))]
    public interface IStatefulMessageBus : IMessageBus
    {
        TResponse FutureSend<TResponse>(SideEffectFreeRequest<TResponse> request)
            where TResponse : Response, new();

        bool HasAlreadySentForFutures { get; }

        bool AllowMultipleFutureSendBatches { get; set; }
    }
}
