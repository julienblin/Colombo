using System;
using System.Diagnostics.Contracts;

namespace Colombo
{
    [ContractClass(typeof(Contracts.AsyncCallbackContract<>))]
    public interface IAsyncCallback<out TResponse>
        where TResponse : Response, new()
    {
        void Register(Action<TResponse> theCallback);

        void Register(Action<TResponse> theCallback, Action<Exception> theErrorCallback);
    }
}
