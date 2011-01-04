using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Colombo.Impl
{
    public abstract class BaseColomboSingleInvocation : IColomboSingleInvocation
    {
        private readonly BaseRequest request;

        protected BaseColomboSingleInvocation(BaseRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();

            this.request = request;
        }

        public BaseRequest Request
        {
            get { return request; }
        }

        public Response Response { get; set; }

        public abstract void Proceed();
    }
}
