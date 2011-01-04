using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Colombo.Wcf
{
    internal class ParallelSendAction
    {
        private readonly BaseRequest[] requests;
        private readonly WcfClientBaseService clientBase;
        private Dictionary<BaseRequest, Response> responsesMap = new Dictionary<BaseRequest, Response>();

        public ParallelSendAction(BaseRequest[] requests, WcfClientBaseService clientBase)
        {
            if (requests == null) throw new ArgumentNullException("requests");
            Contract.EndContractBlock();

            this.requests = requests;
            this.clientBase = clientBase;
        }

        public void Execute()
        {
            Contract.Assume(requests != null);
            using (clientBase)
            {
                Response[] responses = clientBase.ParallelSend(requests);
                for (int i = 0; i < requests.Length; i++)
                {
                    responsesMap.Add(requests[i], responses[i]);
                }
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
