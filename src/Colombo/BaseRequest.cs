using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Colombo
{
    /// <summary>
    /// Base class for requests - non generic version.
    /// </summary>
    public abstract class BaseRequest : BaseMessage
    {
        private IDictionary<string, string> context;
        /// <summary>
        /// Context of the request. Garanteed to be non-null.
        /// </summary>
        public IDictionary<string, string> Context
        {
            get
            {
                if(context == null)
                    context = new Dictionary<string, string>();
                return context;
            }
            set
            {
                if(value == null) throw new ArgumentNullException("Context");
                Contract.EndContractBlock();
                context = value;
            }
        }

        /// <summary>
        /// Get the name of the group that this request belongs to.
        /// </summary>
        /// <remarks>
        /// Defaults to the name of the assembly.
        /// </remarks>
        /// <returns></returns>
        public virtual string GetGroupName()
        {
            return GetType().Assembly.GetName().Name;
        }

        /// <summary>
        /// Get the System.Type of the response associated with this request.
        /// </summary>
        /// <returns></returns>
        public abstract Type GetResponseType();

        /// <summary>
        /// Create a Response object that is related to the current request.
        /// </summary>
        public virtual Response CreateResponse()
        {
            var responseType = GetResponseType();
            if((responseType == null) || (!typeof(Response).IsAssignableFrom(responseType)))
                throw new ColomboException(string.Format("Unable to create a response from response type {0}: either null or not derived from Response.", responseType));
            
            return (Response)Activator.CreateInstance(responseType);
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
}
