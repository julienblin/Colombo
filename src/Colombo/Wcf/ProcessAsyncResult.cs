#region License
// The MIT License
// 
// Copyright (c) 2011 Julien Blin, julien.blin@gmail.com
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
#endregion

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
