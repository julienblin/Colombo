using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Colombo.Impl
{
    public class RequestHandlerColomboInvocation : BaseColomboInvocation
    {
        private readonly IRequestHandler requestHandler;

        public RequestHandlerColomboInvocation(ColomboInvocationType invocationType, BaseRequest request, IRequestHandler requestHandler)
            : base(invocationType, request)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (requestHandler == null) throw new ArgumentNullException("requestHandler");
            Contract.EndContractBlock();

            this.requestHandler = requestHandler;
        }

        public override void Proceed()
        {
            Contract.Assume(Request != null);
            Response = requestHandler.Handle(Request);
        }
    }
}
