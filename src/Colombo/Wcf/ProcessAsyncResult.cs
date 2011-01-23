using System;
using System.Threading;

namespace Colombo.Wcf
{
    /// <summary>
    /// Implementation of <see cref="IAsyncResult"/> for the Process operation.
    /// </summary>
    public class ProcessAsyncResult : IAsyncResult, IDisposable
    {
        private readonly AsyncCallback callback;
        private readonly object state;
        private readonly ManualResetEvent manualResetEvent;

        /// <summary>
        /// Constructor.
        /// </summary>
        public ProcessAsyncResult(AsyncCallback callback, object state)
        {
            this.callback = callback;
            this.state = state;
            manualResetEvent = new ManualResetEvent(false);
        }

        /// <summary>
        /// Incoming requests
        /// </summary>
        public BaseRequest[] Requests { get; set; }

        /// <summary>
        /// Outgoing responses
        /// </summary>
        public Response[] Responses { get; set; }

        /// <summary>
        /// An eventual exception that occured
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// Call this method to set the WaitHandle and invoke the callback.
        /// </summary>
        public virtual void OnCompleted()
        {
            manualResetEvent.Set();
            if(callback != null)
                callback(this);
        }

        /// <summary>
        /// Gets a user-defined object that qualifies or contains information about an asynchronous operation.
        /// </summary>
        /// <returns>
        /// A user-defined object that qualifies or contains information about an asynchronous operation.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public object AsyncState
        {
            get { return state; }
        }


        /// <summary>
        /// Gets a <see cref="T:System.Threading.WaitHandle"/> that is used to wait for an asynchronous operation to complete.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Threading.WaitHandle"/> that is used to wait for an asynchronous operation to complete.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public WaitHandle AsyncWaitHandle
        {
            get { return manualResetEvent; }
        }

        /// <summary>
        /// Gets a value that indicates whether the asynchronous operation completed synchronously.
        /// </summary>
        /// <returns>
        /// true if the asynchronous operation completed synchronously; otherwise, false.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public bool CompletedSynchronously
        {
            get { return false; }
        }

        /// <summary>
        /// Gets a value that indicates whether the asynchronous operation has completed.
        /// </summary>
        /// <returns>
        /// true if the operation is complete; otherwise, false.
        /// </returns>
        /// <filterpriority>2</filterpriority>
        public bool IsCompleted
        {
            get { return manualResetEvent.WaitOne(0, false); }
        }

        private bool disposed;

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        /// <filterpriority>2</filterpriority>
        public void Dispose()
        {
            if (disposed) return;

            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (manualResetEvent != null)
                        manualResetEvent.Dispose();
                }
            }
            finally
            {
                disposed = true;
            }
        }

        /// <summary>
        /// Destructor
        /// </summary>
        ~ProcessAsyncResult()
        {
            Dispose(false);
        }
    }
}
