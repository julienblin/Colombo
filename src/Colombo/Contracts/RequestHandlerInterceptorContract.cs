using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Colombo.Contracts
{
    [ContractClassFor(typeof(IRequestHandlerInterceptor))]
    public abstract class RequestHandlerInterceptorContract : IRequestHandlerInterceptor
    {
        public Response BeforeHandle(BaseRequest request)
        {
            Contract.Requires<ArgumentNullException>(request != null, "request");
            return default(Response);
        }

        public void AfterHandle(BaseRequest request, Response response)
        {
            Contract.Requires<ArgumentNullException>(request != null, "request");
            Contract.Requires<ArgumentNullException>(response != null, "response");
        }

        public int InterceptionPriority
        {
            get { return default(int); }
        }
    }
}
