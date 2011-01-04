using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Colombo.Impl
{
    public class MessageProcessorParallelColomboInvocation : BaseColomboParallelInvocation
    {
        private readonly IMessageProcessor processor;

        public MessageProcessorParallelColomboInvocation(IMessageProcessor processor)
        {
            this.processor = processor;
        }

        public override void Proceed()
        {
            Responses = processor.ParallelSend(Requests);
        }
    }
}
