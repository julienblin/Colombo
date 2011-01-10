using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Colombo.Contracts
{
    [ContractClassFor(typeof(IAsyncCallback<>))]
    public abstract class AsyncCallbackContract<TResponse> : IAsyncCallback<TResponse>
        where TResponse : Response, new()
    {
        public void Register(Action<TResponse> callback)
        {
            Contract.Requires<ArgumentNullException>(callback != null, "callback");
            throw new NotImplementedException();
        }

        public void Register(Action<TResponse> callback, Action<Exception> errorCallback)
        {
            Contract.Requires<ArgumentNullException>(callback != null, "callback");
            throw new NotImplementedException();
        }
    }
}
