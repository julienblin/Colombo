using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Colombo.Impl
{
    public class InterceptorColomboParallelInvocation : BaseColomboParallelInvocation
    {
        private readonly IColomboParallelInterceptor interceptor;
        private readonly IColomboParallelInvocation nextInvocation;

        public InterceptorColomboParallelInvocation(IColomboParallelInterceptor interceptor, IColomboParallelInvocation nextInvocation)
        {
            if (interceptor == null) throw new ArgumentNullException("interceptor");
            if (nextInvocation == null) throw new ArgumentNullException("nextInvocation");
            Contract.EndContractBlock();

            this.interceptor = interceptor;
            this.nextInvocation = nextInvocation;
        }

        public override void Proceed()
        {
            nextInvocation.Requests = Requests;
            interceptor.Intercept(nextInvocation);
            Responses = nextInvocation.Responses;
        }
    }
}
