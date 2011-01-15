using System;
using System.Diagnostics.Contracts;

namespace Colombo
{
    /// <summary>
    /// Allows the registration of a callback function that will be called when a response arrive.
    /// </summary>
    /// <remarks>
    /// Used with the IMessageBus.SendAsync method.
    /// </remarks>
    [ContractClass(typeof(Contracts.AsyncCallbackContract<>))]
    public interface IAsyncCallback<out TResponse>
        where TResponse : Response, new()
    {
        /// <summary>
        /// Register a callback function that will be called when the response arrived.
        /// Be careful because you will NOT be notified of a failure (Exception).
        /// </summary>
        /// <param name="theCallback">The function to call when a response arrives.</param>
        void Register(Action<TResponse> theCallback);

        /// <summary>
        /// Register a callback function that will be called when the response arrived,
        /// and an error callback that will be called if a failure happens.
        /// Either one of these callbacks will be called, but not both.
        /// </summary>
        /// <param name="theCallback">The function to call when a response arrives.</param>
        /// <param name="theErrorCallback">The function to call when an exception happens.</param>
        void Register(Action<TResponse> theCallback, Action<Exception> theErrorCallback);
    }
}
