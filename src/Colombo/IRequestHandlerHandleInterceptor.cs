﻿#region License
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

using System.Diagnostics.Contracts;
using Colombo.Contracts;

namespace Colombo
{
    /// <summary>
    /// Interceptor for the <see cref="IRequestHandler.Handle"/> operation.
    /// </summary>
    [ContractClass(typeof(RequestHandlerHandleInterceptorContract))]
    public interface IRequestHandlerHandleInterceptor : IColomboInterceptor
    {
        /// <summary>
        /// Called when the interceptor is asked to intercept.
        /// Must call <see cref="IColomboRequestHandleInvocation.Proceed()"/> to allow the invocation chain to continue.
        /// </summary>
        /// <param name="nextInvocation">The next invocation to proceed.</param>
        void Intercept(IColomboRequestHandleInvocation nextInvocation);
    }
}
