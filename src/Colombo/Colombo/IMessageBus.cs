using System;
using System.Diagnostics.Contracts;

namespace Colombo
{
    /// <summary>
    /// Allow the sending of messages through Colombo (requests and notifications).
    /// </summary>
    [ContractClass(typeof(Contracts.MessageBusContract))]
    public interface IMessageBus
    {
        /// <summary>
        /// Send synchronously a request and returns the response.
        /// </summary>
        Response Send(BaseRequest request);

        /// <summary>
        /// Send synchronously a request and returns the response.
        /// </summary>
        TResponse Send<TResponse>(Request<TResponse> request)
            where TResponse : Response, new();

        /// <summary>
        /// Send a request asynchronously. You must register a callback with the result to get the response or the error.
        /// </summary>
        IAsyncCallback<TResponse> SendAsync<TResponse>(Request<TResponse> request)
            where TResponse : Response, new();

        /// <summary>
        /// Send synchronously a request and returns the response.
        /// </summary>
        TResponse Send<TResponse>(SideEffectFreeRequest<TResponse> request)
            where TResponse : Response, new();

        /// <summary>
        /// Send synchronously a request and returns the response.
        /// </summary>
        TResponse Send<TRequest, TResponse>(Action<TRequest> action)
            where TRequest : SideEffectFreeRequest<TResponse>, new()
            where TResponse : Response, new();

        /// <summary>
        /// Send a request asynchronously. You must register a callback with the result to get the response or the error.
        /// </summary>
        IAsyncCallback<TResponse> SendAsync<TResponse>(SideEffectFreeRequest<TResponse> request)
            where TResponse : Response, new();

        /// <summary>
        /// Send synchronously, but in parallel, several requests and returns all the responses at once.
        /// Only side effect-free requests can be parallelized.
        /// </summary>
        ResponsesGroup Send(BaseSideEffectFreeRequest request, params BaseSideEffectFreeRequest[] followingRequests);

        /// <summary>
        /// Dispatch notifications
        /// </summary>
        void Notify(Notification notification, params Notification[] notifications);
    }
}
