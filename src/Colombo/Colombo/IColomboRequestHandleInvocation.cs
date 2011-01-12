using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Colombo
{
    /// <summary>
    /// Represents an invocation for the Handle operation.
    /// </summary>
    public interface IColomboRequestHandleInvocation
    {
        /// <summary>
        /// The request to handle.
        /// </summary>
        BaseRequest Request { get; set; }

        /// <summary>
        /// The response.
        /// </summary>
        Response Response { get; set; }

        void Proceed();
    }
}
