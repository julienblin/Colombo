using System;
using System.Diagnostics.Contracts;

namespace Colombo
{
    [ContractClass(typeof(Contracts.AsyncCallbackContract<>))]
    public interface IAsyncCallback<TResponse>
        where TResponse : Response, new()
    {
        void Register(Action<TResponse> callback);

        void Register(Action<TResponse> callback, Action<Exception> errorCallback);
    }
}
