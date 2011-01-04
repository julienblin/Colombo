using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Colombo.Impl
{
    internal class ParallelSendAction
    {
        private readonly IMessageProcessor messageProcessor;
        private readonly BaseRequest[] requests;
        private Dictionary<BaseRequest, Response> responsesMap = new Dictionary<BaseRequest,Response>();

        public ParallelSendAction(IMessageProcessor messageProcessor, BaseRequest[] requests)
        {
            if (messageProcessor == null) throw new ArgumentNullException("messageProcessor");
            if (requests == null) throw new ArgumentNullException("requests");
            Contract.EndContractBlock();

            this.messageProcessor = messageProcessor;
            this.requests = requests;
        }

        public void Execute()
        {
            Contract.Assume(requests != null);
            var responses = messageProcessor.ParallelSend(requests);
            for (int i = 0; i < requests.Length; i++)
            {
                responsesMap.Add(requests[i], responses[i]);
            }
        }

        public Response GetResponseFor(BaseRequest request)
        {
            if (responsesMap.ContainsKey(request))
                return responsesMap[request];

            return null;
        }
    }
}
