using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Colombo
{
    [ContractClass(typeof(Contracts.MessageBusContract))]
    public interface IMessageBus
    {
        /// <summary>
        /// Send synchronously a request and returns the response.
        /// </summary>
        TResponse Send<TResponse>(Request<TResponse> request)
            where TResponse : Response, new();

        IAsyncCallback<TResponse> SendAsync<TResponse>(Request<TResponse> request)
            where TResponse : Response, new();

        /// <summary>
        /// Send synchronously a request and returns the response.
        /// </summary>
        TResponse Send<TResponse>(SideEffectFreeRequest<TResponse> request)
            where TResponse : Response, new();

        IAsyncCallback<TResponse> SendAsync<TResponse>(SideEffectFreeRequest<TResponse> request)
            where TResponse : Response, new();

        /// <summary>
        /// Send synchronously, but in parallel, several requests and returns all the responses at once.
        /// Only side effect-free requests can be parallelized.
        /// </summary>
        ResponsesGroup Send(BaseSideEffectFreeRequest request, params BaseSideEffectFreeRequest[] followingRequests);

    }
}
