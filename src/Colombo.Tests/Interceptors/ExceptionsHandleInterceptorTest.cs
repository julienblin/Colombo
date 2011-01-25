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
using Castle.Core.Logging;
using Colombo.Alerts;
using Colombo.Interceptors;
using NUnit.Framework;
using Rhino.Mocks;
using Is = Rhino.Mocks.Constraints.Is;

namespace Colombo.Tests.Interceptors
{
    [TestFixture]
    public class ExceptionsHandleInterceptorTest
    {
        [Test]
        public void It_should_not_alert_when_no_exception_occurs()
        {
            var mocks = new MockRepository();
            var logger = mocks.DynamicMock<ILogger>();

            var invocation = mocks.StrictMock<IColomboRequestHandleInvocation>();
            var request = new TestRequest();

            var alerter1 = mocks.StrictMock<IColomboAlerter>();

            With.Mocks(mocks).Expecting(() =>
            {
                SetupResult.For(invocation.Request).Return(request);
                invocation.Proceed();
                LastCall.Do(new ProceedDelegate(() =>
                {

                }));

            }).Verify(() =>
            {
                var interceptor = new ExceptionsHandleInterceptor();
                interceptor.Logger = logger;
                interceptor.Alerters = new[] { alerter1 };
                interceptor.Intercept(invocation);
            });
        }

        [Test]
        public void It_should_log_warn_if_exception_raised()
        {
            var mocks = new MockRepository();
            var logger = mocks.DynamicMock<ILogger>();

            var invocation = mocks.StrictMock<IColomboRequestHandleInvocation>();
            var request = new TestRequest();

            With.Mocks(mocks).Expecting(() =>
            {
                SetupResult.For(invocation.Request).Return(request);
                invocation.Proceed();
                LastCall.Do(new ProceedDelegate(() =>
                {
                    throw new Exception("TestException");
                }));
                logger.Warn(null);
                LastCall.IgnoreArguments();
            }).Verify(() =>
            {
                var interceptor = new ExceptionsHandleInterceptor();
                interceptor.Logger = logger;
                Assert.That(() => interceptor.Intercept(invocation),
                            Throws.Exception);
            });
        }

        [Test]
        public void It_should_contact_all_alerters_if_exception_raised()
        {
            var mocks = new MockRepository();
            var logger = mocks.DynamicMock<ILogger>();

            var invocation = mocks.StrictMock<IColomboRequestHandleInvocation>();
            var request = new TestRequest();

            var alerter1 = mocks.StrictMock<IColomboAlerter>();
            var alerter2 = mocks.StrictMock<IColomboAlerter>();

            With.Mocks(mocks).Expecting(() =>
            {
                SetupResult.For(invocation.Request).Return(request);
                invocation.Proceed();
                LastCall.Do(new ProceedDelegate(() =>
                {
                    throw new Exception("TestException");
                }));

                alerter1.Alert(null);
                LastCall.IgnoreArguments().Constraints(
                    Is.TypeOf<ExceptionAlert>()
                );
                alerter2.Alert(null);
                LastCall.IgnoreArguments().Constraints(
                    Is.TypeOf<ExceptionAlert>()
                );
            }).Verify(() =>
            {
                var interceptor = new ExceptionsHandleInterceptor();
                interceptor.Alerters = new[] { alerter1, alerter2 };
                Assert.That(() => interceptor.Intercept(invocation),
                            Throws.Exception);
            });
        }

        public class TestRequest : Request<TestResponse>
        {
        }

        public delegate void ProceedDelegate();
    }
}
