using System;
using System.Diagnostics.Contracts;

namespace Colombo.Impl.Async
{
    public class AsyncCallback<TResponse> : IAsyncCallback<TResponse>
        where TResponse : Response, new()
    {
        private object syncRoot = new object();

        private Action<TResponse> callback;
        private Action<Exception> errorCallback;
        private TResponse response;
        private Exception exception;

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
                this.callback = callback;
                this.errorCallback = errorCallback;
                
                if (this.response != null)
                    callback(response);

                if (this.exception != null)
                    if(this.errorCallback != null)
                        callback(response);
            }
        }

        internal void ResponseArrived(TResponse response)
        {
            lock (syncRoot)
            {
                if (this.callback == null)
                    this.response = response;
                else
                    this.callback(response);
            }
        }

        internal void ExceptionArrived(Exception exception)
        {
            lock (syncRoot)
            {
                if (this.errorCallback == null)
                    this.exception = exception;
                else
                    this.errorCallback(exception);
            }
        }
    }
}
