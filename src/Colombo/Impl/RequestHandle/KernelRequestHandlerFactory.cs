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
using Castle.MicroKernel;

namespace Colombo.Impl.RequestHandle
{
    /// <summary>
    /// Implementation of <see cref="IRequestHandlerFactory"/> that uses <see cref="Castle.MicroKernel.IKernel"/>.
    /// </summary>
    public class KernelRequestHandlerFactory : IRequestHandlerFactory
    {
        private readonly IKernel kernel;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="kernel">Kernel used to resolve the request handlers.</param>
        public KernelRequestHandlerFactory(IKernel kernel)
        {
            if (kernel == null) throw new ArgumentNullException("kernel");
            Contract.EndContractBlock();

            this.kernel = kernel;
        }

        /// <summary>
        /// <c>true</c> if the factory can create a <see cref="IRequestHandler"/> to handle the request, <c>false</c> otherwise.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public bool CanCreateRequestHandlerFor(BaseRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();

            var requestHandlerType = CreateRequestHandlerTypeFrom(request);
            return kernel.HasComponent(requestHandlerType);
        }

        /// <summary>
        /// Creates a <see cref="IRequestHandler"/> to handle the <paramref name="request"/>.
        /// </summary>
        public IRequestHandler CreateRequestHandlerFor(BaseRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();

            var requestHandlerType = CreateRequestHandlerTypeFrom(request);

            try
            {
                return (IRequestHandler)kernel.Resolve(requestHandlerType);
            }
            catch (ComponentNotFoundException)
            {
                throw new ColomboException(string.Format("Request Handler {0} not found for {1}.", requestHandlerType, request));
            }
        }

        /// <summary>
        /// Dispose the <paramref name="requestHandler"/>.
        /// </summary>
        public void DisposeRequestHandler(IRequestHandler requestHandler)
        {
            if (requestHandler == null) throw new ArgumentNullException("requestHandler");
            Contract.EndContractBlock();

            kernel.ReleaseComponent(requestHandler);
        }

        private static Type CreateRequestHandlerTypeFrom(BaseRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();

            var responseType = request.GetResponseType();
            var requestType = request.GetType();

            Contract.Assume(typeof(Request<>).IsGenericTypeDefinition);
            Contract.Assume(typeof(Request<>).GetGenericArguments().Length == 1);
            Contract.Assume(typeof(IRequestHandler<,>).IsGenericTypeDefinition);
            Contract.Assume(typeof(IRequestHandler<,>).GetGenericArguments().Length == 2);
            var stdRequestType = typeof(Request<>).MakeGenericType(request.GetResponseType());
            if (stdRequestType.IsAssignableFrom(requestType))
                return typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);

            Contract.Assume(typeof(SideEffectFreeRequest<>).IsGenericTypeDefinition);
            Contract.Assume(typeof(SideEffectFreeRequest<>).GetGenericArguments().Length == 1);
            Contract.Assume(typeof(ISideEffectFreeRequestHandler<,>).IsGenericTypeDefinition);
            Contract.Assume(typeof(ISideEffectFreeRequestHandler<,>).GetGenericArguments().Length == 2);
            var sideEffectFreeRequestType = typeof(SideEffectFreeRequest<>).MakeGenericType(request.GetResponseType());
            if (sideEffectFreeRequestType.IsAssignableFrom(requestType))
                return typeof(ISideEffectFreeRequestHandler<,>).MakeGenericType(requestType, responseType);

            throw new ColomboException(string.Format("Internal error: unable to create request handler generic type for: {0}", request));
        }
    }
}
