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

namespace Colombo.TestSupport
{
    /// <summary>
    /// Expectation for the Send operation.
    /// </summary>
    public class MessageBusSendExpectation<TRequest, TResponse> : BaseExpectation
        where TRequest : BaseRequest, new()
        where TResponse : Response, new()
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public MessageBusSendExpectation(IStubMessageBus stubMessageBus)
            : base(stubMessageBus)
        {
            ExpectedNumCalled = 1;
        }

        private Action<TRequest, TResponse> recordedAction;

        /// <summary>
        /// Allow to specify some actions that will be used to Reply to the Send operation.
        /// </summary>
        /// <param name="action"></param>
        /// <returns></returns>
        public MessageBusSendExpectation<TRequest, TResponse> Reply(Action<TRequest, TResponse> action)
        {
            recordedAction = action;
            return this;
        }

        /// <summary>
        /// Indicates that this expectation should be repeated n <paramref name="times"/>.
        /// </summary>
        public MessageBusSendExpectation<TRequest, TResponse> Repeat(int times)
        {
            ExpectedNumCalled = times;
            return this;
        }

        internal override object Execute(object parameter)
        {
            ++NumCalled;
            var request = (TRequest)parameter;
            if (recordedAction != null)
            {
                var response = new TResponse { CorrelationGuid = request.CorrelationGuid };
                recordedAction.Invoke(request, response);
                return response;
            }
            return null;
        }

        /// <summary>
        /// Verify that all the operations meet what this expectation planned.
        /// </summary>
        public override void Verify()
        {
            if (ExpectedNumCalled != NumCalled)
                throw new ColomboExpectationException(string.Format("Expected {0} to be sent {1} time(s), actual: {2}", typeof(TRequest), ExpectedNumCalled, NumCalled));
        }
    }
}
