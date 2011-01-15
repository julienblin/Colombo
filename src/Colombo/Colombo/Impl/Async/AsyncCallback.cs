using System;
using System.Diagnostics.Contracts;

namespace Colombo.Impl.Async
{
    public class AsyncCallback<TResponse> : IAsyncCallback<TResponse>
        where TResponse : Response, new()
    {
        private readonly object syncRoot = new object();

        private Action<TResponse> callback;
        private Action<Exception> errorCallback;
        private TResponse response;
        private Exception exception;

        public void Register(Action<TResponse> theCallback)
        {
            if (theCallback == null) throw new ArgumentNullException("theCallback");
            Contract.EndContractBlock();

            Register(theCallback, null);
        }

        public void Register(Action<TResponse> theCallback, Action<Exception> theErrorCallback)
        {
            if (theCallback == null) throw new ArgumentNullException("theCallback");
            Contract.EndContractBlock();

            lock (syncRoot)
            {
                this.callback = theCallback;
                this.errorCallback = theErrorCallback;
                
                if (this.response != null)
                    theCallback(response);

                if (this.exception != null)
                    if(this.errorCallback != null)
                        theCallback(response);
            }
        }

        internal void ResponseArrived(TResponse theResponse)
        {
            lock (syncRoot)
            {
                if (this.callback == null)
                    this.response = theResponse;
                else
                    this.callback(theResponse);
            }
        }

        internal void ExceptionArrived(Exception theException)
        {
            lock (syncRoot)
            {
                if (this.errorCallback == null)
                    this.exception = theException;
                else
                    this.errorCallback(theException);
            }
        }
    }
}
