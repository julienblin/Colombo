using System;
using System.Diagnostics.Contracts;

namespace Colombo.Contracts
{
    [ContractClassFor(typeof(IAsyncCallback<>))]
    public abstract class AsyncCallbackContract<TResponse> : IAsyncCallback<TResponse>
        where TResponse : Response, new()
    {
        public void Register(Action<TResponse> theCallback)
        {
            Contract.Requires<ArgumentNullException>(theCallback != null, "callback");
            throw new NotImplementedException();
        }

        public void Register(Action<TResponse> theCallback, Action<Exception> theErrorCallback)
        {
            Contract.Requires<ArgumentNullException>(theCallback != null, "callback");
            throw new NotImplementedException();
        }
    }
}
