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
    /// Represent a component that handles requests.
    /// </summary>
    [ContractClass(typeof(RequestHandlerContract))]
    public interface IRequestHandler
    {
        /// <summary>
        /// Handles the request.
        /// </summary>
        Response Handle(BaseRequest request);

        /// <summary>
        /// Get the type of request that this request handler handles.
        /// </summary>
        /// <returns></returns>
        Type GetRequestType();

        /// <summary>
        /// Get the type of response that this request handler produces.
        /// </summary>
        /// <returns></returns>
        Type GetResponseType();
    }

    /// <summary>
    /// Interface used to defined request handlers for requests.
    /// You may prefer to use the abstract RequestHandler class instead.
    /// </summary>
    [ContractClass(typeof(GenericRequestHandlerContract<,>))]
    public interface IRequestHandler<in TRequest, out TResponse> : IRequestHandler
        where TResponse : Response, new()
        where TRequest : Request<TResponse>, new()
    {
        /// <summary>
        /// Handles the request.
        /// </summary>
        TResponse Handle(TRequest request);
    }


}
