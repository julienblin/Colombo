using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Colombo.Contracts
{
    [ContractClassFor(typeof(IRequestProcessor))]
    public abstract class RequestProcessorContract : IRequestProcessor
    {
        public bool CanProcess(BaseRequest request)
        {
            Contract.Requires<ArgumentNullException>(request != null, "request");
            throw new NotImplementedException();
        }

        public ResponsesGroup Process(IList<BaseRequest> requests)
        {
            Contract.Requires<ArgumentNullException>(requests != null, "requests");
            Contract.Ensures(Contract.Result<ResponsesGroup>() != null);
            Contract.Ensures(Contract.Result<ResponsesGroup>().Count == requests.Count);
            throw new NotImplementedException();
        }
    }
}
