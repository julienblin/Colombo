using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Colombo.Impl
{
    public class MessageProcessorColomboInvocation : BaseColomboInvocation
    {
        private readonly IMessageProcessor processor;

        public MessageProcessorColomboInvocation(BaseRequest request, IMessageProcessor processor)
            : base(request)
        {
            if (request == null) throw new ArgumentNullException("request");
            if (processor == null) throw new ArgumentNullException("processor");
            Contract.EndContractBlock();

            this.processor = processor;
        }

        public override void Proceed()
        {
            Contract.Assume(Request != null);
            Response = processor.Send(Request);
        }
    }
}
