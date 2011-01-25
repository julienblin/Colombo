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
using Colombo.Contracts;

namespace Colombo
{
    /// <summary>
    /// Allows the registration of a callback function that will be called when a response arrive.
    /// </summary>
    /// <remarks>
    /// Used with the IMessageBus.SendAsync method.
    /// </remarks>
    [ContractClass(typeof(AsyncCallbackContract<>))]
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
