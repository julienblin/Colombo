using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Colombo
{
    /// <summary>
    /// Base class for requests.
    /// </summary>
    /// <typeparam name="TResponse">The type of the associated response.</typeparam>
    public abstract class Request<TResponse> : BaseRequest
        where TResponse : Response, new()
    {
        public override Type GetResponseType()
        {
            return typeof(TResponse);
        }
    }
}
