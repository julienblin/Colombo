using System;
using System.Diagnostics.Contracts;

namespace Colombo.Impl.RequestHandle
{
    internal class RequestHandlerHandleInterceptorInvocation : BaseRequestHandleInvocation
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
