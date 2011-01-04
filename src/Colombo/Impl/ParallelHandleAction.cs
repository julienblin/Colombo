using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Colombo.Impl
{
    public class ParallelHandleAction
    {
        private readonly ILocalMessageProcessor processor;
        private readonly BaseRequest request;

        public ParallelHandleAction(ILocalMessageProcessor processor, BaseRequest request)
        {
            this.processor = processor;
            this.request = request;
        }

        public void Execute()
        {
            Response = processor.Send(request);
        }

        public Response Response { get; private set; }
    }
}
