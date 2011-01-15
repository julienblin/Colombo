using System;
using System.Diagnostics.Contracts;

namespace Colombo.Impl.Async
{
    /// <summary>
    /// IAsyncCallback implementation.
    /// </summary>
    public class AsyncCallback<TResponse> : IAsyncCallback<TResponse>
        where TResponse : Response, new()
    {
        private readonly object syncRoot = new object();

        private Action<TResponse> registeredCallback;
        private Action<Exception> registeredErrorCallback;
        private TResponse arrivedResponse;
        private Exception arrivedException;

        /// <summary>
        /// Register a callback function that will be called when the response arrived.
        /// Be careful because you will NOT be notified of a failure (Exception).
        /// </summary>
        /// <param name="theCallback">The function to call when a response arrives.</param>
        public void Register(Action<TResponse> callback)
        {
            if (callback == null) throw new ArgumentNullException("callback");
            Contract.EndContractBlock();

            Register(callback, null);
        }

        /// <summary>
        /// Register a callback function that will be called when the response arrived,
        /// and an error callback that will be called if a failure happens.
        /// Either one of these callbacks will be called, but not both.
        /// </summary>
        /// <param name="theCallback">The function to call when a response arrives.</param>
        /// <param name="theErrorCallback">The function to call when an exception happens.</param>
        public void Register(Action<TResponse> callback, Action<Exception> errorCallback)
        {
            if (callback == null) throw new ArgumentNullException("callback");
            Contract.EndContractBlock();

            lock (syncRoot)
            {
                registeredCallback = callback;
                registeredErrorCallback = errorCallback;
                
                if (arrivedResponse != null)
                    callback(arrivedResponse);

                if (arrivedException != null)
                    if(registeredErrorCallback != null)
                        callback(arrivedResponse);
            }
        }

        internal void ResponseArrived(TResponse response)
        {
            lock (syncRoot)
            {
                if (registeredCallback == null)
                    arrivedResponse = response;
                else
                    registeredCallback(response);
            }
        }

        internal void ExceptionArrived(Exception exception)
        {
            lock (syncRoot)
            {
                if (registeredErrorCallback == null)
                    arrivedException = exception;
                else
                    registeredErrorCallback(exception);
            }
        }
    }
}
