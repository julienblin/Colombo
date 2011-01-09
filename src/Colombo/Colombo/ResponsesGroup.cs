using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Colombo
{
    /// <summary>
    /// Represents a group of responses, index by the request that created them.
    /// </summary>
    public class ResponsesGroup : Dictionary<BaseRequest, Response>
    {
        /// <summary>
        /// Returns the response associated with the request.
        /// </summary>
        public TResponse GetFrom<TResponse>(SideEffectFreeRequest<TResponse> request)
            where TResponse : Response, new()
        {
            return (TResponse)this[request];
        }
    }
}
