using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Colombo.Impl
{
    public class RequestHandlerHandleInterceptorInvocation : BaseHandleInvocation
    {
        private readonly IRequestHandlerHandleInterceptor interceptor;
        private readonly IColomboHandleInvocation nextInvocation;

        public RequestHandlerHandleInterceptorInvocation(IRequestHandlerHandleInterceptor interceptor, IColomboHandleInvocation nextInvocation)
        {
            if (interceptor == null) throw new ArgumentNullException("interceptor");
            if (nextInvocation == null) throw new ArgumentNullException("nextInvocation");
            Contract.EndContractBlock();

            this.interceptor = interceptor;
            this.nextInvocation = nextInvocation;
        }

        public override void Proceed()
        {
            nextInvocation.Request = Request;
            interceptor.Intercept(nextInvocation);
            Response = nextInvocation.Response;
        }
    }
}
