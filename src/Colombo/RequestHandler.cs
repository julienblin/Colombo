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

namespace Colombo
{
    /// <summary>
    /// Use this base class to create request handlers for standard requests.
    /// </summary>
    public abstract class RequestHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
        where TResponse : Response, new()
        where TRequest : Request<TResponse>, new()
    {
        /// <summary>
        /// Incoming request.
        /// </summary>
        protected TRequest Request { get; private set; }

        /// <summary>
        /// Outgoing response. It will be created before Handle() and the CorrelationGuid will be set.
        /// </summary>
        protected TResponse Response { get; set; }

        /// <summary>
        /// Handles the request.
        /// </summary>
        public virtual Response Handle(BaseRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();

            return Handle((TRequest)request);
        }

        /// <summary>
        /// Handles the request.
        /// </summary>
        public virtual TResponse Handle(TRequest request)
        {
            Request = request;
            Response = new TResponse { CorrelationGuid = request.CorrelationGuid };
            Handle();
            return Response;
        }

        /// <summary>
        /// Handles the request.
        /// </summary>
        protected abstract void Handle();

        /// <summary>
        /// Create a new request to be used inside this request handler.
        /// The CorrelationGuid and the Context are copied.
        /// </summary>
        protected TNewRequest CreateRequest<TNewRequest>()
            where TNewRequest : BaseRequest, new()
        {
            var result = new TNewRequest { CorrelationGuid = Request.CorrelationGuid, Context = Request.Context };
            return result;
        }

        /// <summary>
        /// Get the type of request that this request handler handles.
        /// </summary>
        /// <returns></returns>
        public Type GetRequestType()
        {
            return typeof(TRequest);
        }

        /// <summary>
        /// Get the type of response that this request handler produces.
        /// </summary>
        /// <returns></returns>
        public Type GetResponseType()
        {
            return typeof(TResponse);
        }
    }
}
