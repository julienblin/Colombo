using System.Collections.Generic;

namespace Colombo
{
    /// <summary>
    /// Represents an invocation for the Send operation.
    /// </summary>
    public interface IColomboSendInvocation
    {
        /// <summary>
        /// The list of requests that will be send or that have been sent.
        /// </summary>
        IList<BaseRequest> Requests { get; set; }

        /// <summary>
        /// The responses at this moment.
        /// </summary>
        ResponsesGroup Responses { get; set; }

        /// <summary>
        /// Proceed with the execution of the invocation.
        /// </summary>
        void Proceed();
    }
}
