namespace Colombo
{
    /// <summary>
    /// Invocation used in the interception process for handling requests.
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

        /// <summary>
        /// Proceed with the invocation.
        /// </summary>
        void Proceed();
    }
}
