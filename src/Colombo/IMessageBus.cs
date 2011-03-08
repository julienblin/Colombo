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
    /// Allow the sending of messages through Colombo.
    /// </summary>
    [ContractClass(typeof(MessageBusContract))]
    public interface IMessageBus
    {
        /// <summary>
        /// Send synchronously a request and returns the response.
        /// </summary>
        Response Send(BaseRequest request);

        /// <summary>
        /// Send synchronously a request and returns the response.
        /// </summary>
        TResponse Send<TResponse>(Request<TResponse> request)
            where TResponse : Response, new();

        /// <summary>
        /// Send a request asynchronously. You must register a callback with the result to get the response or the error.
        /// </summary>
        IAsyncCallback<TResponse> SendAsync<TResponse>(Request<TResponse> request)
            where TResponse : Response, new();

        /// <summary>
        /// Send synchronously a request and returns the response.
        /// </summary>
        TResponse Send<TResponse>(SideEffectFreeRequest<TResponse> request)
            where TResponse : Response, new();

        /// <summary>
        /// Send a request asynchronously. You must register a callback with the result to get the response or the error.
        /// </summary>
        IAsyncCallback<TResponse> SendAsync<TResponse>(SideEffectFreeRequest<TResponse> request)
            where TResponse : Response, new();

        /// <summary>
        /// Send synchronously, but in parallel, several requests and returns all the responses at once.
        /// Only side effect-free requests can be parallelized.
        /// </summary>
        ResponsesGroup Send(BaseSideEffectFreeRequest request, params BaseSideEffectFreeRequest[] followingRequests);
    }
}
