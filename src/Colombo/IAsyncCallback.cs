using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Colombo
{
    public interface IAsyncCallback<TResponse>
        where TResponse : Response, new()
    {
        void Register(Action<TResponse> callback);
    }
}
