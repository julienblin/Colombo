using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Colombo
{
    public abstract class Request<TResponse> : Message
        where TResponse : Response, new()
    {
        protected Request()
        {
            CallContext = new CallContext();
        }

        public CallContext CallContext { get; set; }
    }
}
