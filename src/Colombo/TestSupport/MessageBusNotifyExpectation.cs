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
    /// Expectation for the Notify operation.
    /// </summary>
    /// <typeparam name="TNotification">The type of Notification that should be notified.</typeparam>
    public class MessageBusNotifyExpectation<TNotification> : BaseExpectation
        where TNotification : Notification, new()
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public MessageBusNotifyExpectation(IStubMessageBus stubMessageBus)
            : base(stubMessageBus)
        {
            ExpectedNumCalled = 1;
        }

        private TNotification receivedNotification;

        internal override object Execute(object parameter)
        {
            ++NumCalled;
            receivedNotification = (TNotification) parameter;
            return null;
        }

        /// <summary>
        /// Indicates that this expectation should be repeated n <paramref name="times"/>.
        /// </summary>
        public MessageBusNotifyExpectation<TNotification> Repeat(int times)
        {
            ExpectedNumCalled = times;
            return this;
        }

        private Action<TNotification> assertion;

        /// <summary>
        /// Allows to specify an action to assert certain properties on the notification.
        /// These asserts will be executed when <see cref="Verify"/> is called.
        /// </summary>
        public MessageBusNotifyExpectation<TNotification> Assert(Action<TNotification> action)
        {
            assertion = action;
            return this;
        }

        /// <summary>
        /// Verify that all the operations meet what this expectation planned.
        /// </summary>
        public override void Verify()
        {
            if (ExpectedNumCalled != NumCalled)
                throw new ColomboExpectationException(string.Format("Expected {0} to notify {1} time(s), actual: {2}", typeof(TNotification), ExpectedNumCalled, NumCalled));

            if((receivedNotification != null) && (assertion != null))
                assertion.Invoke(receivedNotification);
        }
    }
}
