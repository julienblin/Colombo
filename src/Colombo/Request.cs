using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Colombo
{
    public abstract class BaseRequest : Message
    {
        protected BaseRequest()
        {
            CallContext = new CallContext();
        }

        public CallContext CallContext { get; set; }

        public abstract Type GetResponseType();
    }

    public abstract class Request<TResponse> : BaseRequest
        where TResponse : Response, new()
    {
        public override Type GetResponseType()
        {
            return typeof(TResponse);
        }
    }
}
