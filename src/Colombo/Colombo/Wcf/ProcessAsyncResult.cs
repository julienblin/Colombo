using System;
using System.Threading;

namespace Colombo.Wcf
{
    public class ProcessAsyncResult : IAsyncResult, IDisposable
    {
        private readonly AsyncCallback callback;
        private readonly object state;
        private readonly ManualResetEvent manualResetEvent;

        public ProcessAsyncResult(AsyncCallback callback, object state)
        {
            this.callback = callback;
            this.state = state;
            manualResetEvent = new ManualResetEvent(false);
        }

        public BaseRequest[] Requests { get; set; }

        public Response[] Responses { get; set; }

        public Exception Exception { get; set; }

        public virtual void OnCompleted()
        {
            manualResetEvent.Set();
            if(callback != null)
                callback(this);
        }

        public object AsyncState
        {
            get { return state; }
        }

        public WaitHandle AsyncWaitHandle
        {
            get { return manualResetEvent; }
        }

        public bool CompletedSynchronously
        {
            get { return false; }
        }

        public bool IsCompleted
        {
            get { return manualResetEvent.WaitOne(0, false); }
        }

        private bool disposed;

        public void Dispose()
        {
            if (disposed) return;

            Dispose(true);
            GC.SuppressFinalize(this);
        }

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

        ~ProcessAsyncResult()
        {
            Dispose(false);
        }
    }
}
