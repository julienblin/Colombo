using System;
using System.Diagnostics.Contracts;

namespace Colombo.Impl.Async
{
    public class AsyncCallback<TResponse> : IAsyncCallback<TResponse>
        where TResponse : Response, new()
    {
        private readonly object syncRoot = new object();

        private Action<TResponse> registeredCallback;
        private Action<Exception> registeredErrorCallback;
        private TResponse arrivedResponse;
        private Exception arrivedException;

        public void Register(Action<TResponse> callback)
        {
            if (callback == null) throw new ArgumentNullException("callback");
            Contract.EndContractBlock();

            Register(callback, null);
        }

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
