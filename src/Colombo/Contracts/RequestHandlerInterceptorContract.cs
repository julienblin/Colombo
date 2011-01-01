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
        public TResponse BeforeHandle<TResponse>(Request<TResponse> request)
            where TResponse : Response, new()
        {
            Contract.Requires<ArgumentNullException>(request != null, "request");
            return default(TResponse);
        }

        public void AfterHandle<TResponse>(Request<TResponse> request, Response response)
            where TResponse : Response, new()
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
