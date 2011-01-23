using System;

namespace Colombo
{
    /// <summary>
    /// Base class for requests.
    /// </summary>
    /// <typeparam name="TResponse">The type of the associated response.</typeparam>
    public abstract class Request<TResponse> : BaseRequest
        where TResponse : Response, new()
    {
        /// <summary>
        /// Get the System.Type of the response associated with this request.
        /// </summary>
        /// <returns></returns>
        public override Type GetResponseType()
        {
            return typeof(TResponse);
        }

        /// <summary>
        /// <c>false</c>
        /// </summary>
        public override bool IsSideEffectFree
        {
            get { return false; }
        }
    }
}
