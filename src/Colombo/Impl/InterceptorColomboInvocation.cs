using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Colombo.Impl
{
    public class InterceptorColomboInvocation : BaseColomboInvocation
    {
        private readonly IColomboInterceptor interceptor;
        private readonly IColomboInvocation nextInvocation;

        public InterceptorColomboInvocation(ColomboInvocationType invocationType, BaseRequest request, IColomboInterceptor interceptor, IColomboInvocation nextInvocation)
            : base(invocationType, request)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (interceptor == null) throw new ArgumentNullException("interceptor");
            if (nextInvocation == null) throw new ArgumentNullException("nextInvocation");
            Contract.EndContractBlock();

            this.interceptor = interceptor;
            this.nextInvocation = nextInvocation;
        }

        public override void Proceed()
        {
            Contract.Assume(nextInvocation != null);

            interceptor.Intercept(nextInvocation);
            Response = nextInvocation.Response;
        }
    }
}
