using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Colombo
{
    /// <summary>
    /// A components that can process requests into responses
    /// </summary>
    public interface IRequestProcessor
    {
        /// <summary>
        /// <c>true</c> if the processor can process the request, <c>false</c> otherwise.
        /// </summary>
        bool CanSend(BaseRequest request);

        /// <summary>
        /// Process the requests.
        /// </summary>
        ResponsesGroup Process(IList<BaseRequest> requests);
    }
}
