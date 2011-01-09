using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Colombo
{
    /// <summary>
    /// Base class for requests that are side-effect free, meaning they don't impact the state of the server.
    /// Use this base class to enable parallelism.
    /// </summary>
    /// <typeparam name="TResponse"></typeparam>
    public abstract class SideEffectFreeRequest<TResponse> : BaseSideEffectFreeRequest
        where TResponse : Response, new()
    {
        public override Type GetResponseType()
        {
            return typeof(TResponse);
        }
    }
}
