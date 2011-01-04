using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Colombo.Impl
{
    public abstract class BaseColomboInvocation : IColomboInvocation
    {
        private readonly ColomboInvocationType invocationType;
        private readonly BaseRequest request;

        protected BaseColomboInvocation(ColomboInvocationType invocationType, BaseRequest request)
        {
            this.invocationType = invocationType;
            this.request = request;
        }

        public BaseRequest Request
        {
            get { return request; }
        }

        public Response Response { get; set; }

        public BaseRequest[] Requests { get; set; }

        public Response[] Responses { get; set; }

        public abstract void Proceed();

        public ColomboInvocationType InvocationType
        {
            get { return invocationType; }
        }
    }
}
