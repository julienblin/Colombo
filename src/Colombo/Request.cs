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
            Context = new Dictionary<string, string>();
        }

        public IDictionary<string, string> Context { get; set; }

        public abstract Type GetResponseType();

        public virtual string GetGroupName()
        {
            return GetType().Assembly.GetName().Name;
        }

        public override string ToString()
        {
            if ((Context != null) && (Context.Count > 0))
            {
                return string.Format("{0} | {1}", base.ToString(), string.Join(", ", Context.Select(x => string.Format("{0}={1}", x.Key, x.Value))));
            }
            else
            {
                return base.ToString();
            }
        }
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
