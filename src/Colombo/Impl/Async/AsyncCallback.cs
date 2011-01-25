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
        /// <param name="callback">The function to call when a response arrives.</param>
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
        /// <param name="callback">The function to call when a response arrives.</param>
        /// <param name="errorCallback">The function to call when an exception happens.</param>
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
