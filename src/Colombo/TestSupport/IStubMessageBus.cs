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
using Castle.MicroKernel;

namespace Colombo.TestSupport
{
    /// <summary>
    /// Allow the setup and verifications of expectations - to be used inside unit tests of handlers.
    /// </summary>
    public interface IStubMessageBus
    {
        /// <summary>
        /// The <see cref="IKernel"/> that will be injected.
        /// </summary>
        IKernel Kernel { get; set; }

        /// <summary>
        /// Indicates a handler type that is under test.
        /// </summary>
        RequestHandlerExpectation<THandler> TestHandler<THandler>()
            where THandler : IRequestHandler;

        /// <summary>
        /// Indicates an expectation that a type of Request will be sent.
        /// </summary>
        MessageBusSendExpectation<TRequest, TResponse> ExpectSend<TRequest, TResponse>()
            where TRequest : BaseRequest, new()
            where TResponse : Response, new();

        /// <summary>
        /// <c>true</c> to allow the <see cref="IStubMessageBus"/> to reply to requests using empty responses,
        /// <c>false</c> to disallow and throw a <see cref="ColomboExpectationException"/> when sending an unexpected request.
        /// </summary>
        bool AllowUnexpectedMessages { get; set; }

        /// <summary>
        /// Returns the <see cref="BaseExpectation"/> associated with the <paramref name="messageType"/>
        /// </summary>
        BaseExpectation GetExpectationFor(Type messageType);

        /// <summary>
        /// Verify all the expectations
        /// </summary>
        /// <exception cref="ColomboExpectationException" />
        void Verify();
    }
}
