using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Colombo
{
    /// <summary>
    /// A components that can process requests into responses
    /// </summary>
    [ContractClass(typeof(Contracts.RequestProcessorContract))]
    public interface IRequestProcessor
    {
        /// <summary>
        /// <c>true</c> if the processor can process the request, <c>false</c> otherwise.
        /// </summary>
        bool CanProcess(BaseRequest request);

        /// <summary>
        /// Process the requests.
        /// </summary>
        ResponsesGroup Process(IList<BaseRequest> requests);
    }
}
