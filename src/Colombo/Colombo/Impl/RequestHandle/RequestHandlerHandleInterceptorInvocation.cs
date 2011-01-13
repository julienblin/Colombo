using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Colombo.Impl.RequestHandle
{
    public class RequestHandlerHandleInterceptorInvocation : BaseRequestHandleInvocation
    {
        private readonly IRequestHandlerHandleInterceptor interceptor;
        private readonly IColomboRequestHandleInvocation nextInvocation;

        public RequestHandlerHandleInterceptorInvocation(IRequestHandlerHandleInterceptor interceptor, IColomboRequestHandleInvocation nextInvocation)
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
