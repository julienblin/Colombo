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

using System.Collections.Generic;
using System.Threading;
using Castle.Core.Logging;
using Colombo.Interceptors;
using NUnit.Framework;
using Rhino.Mocks;

namespace Colombo.Tests.Interceptors
{
    [TestFixture]
    public class SLASendInterceptorTest
    {
        [Test]
        public void It_should_measure_processing_time_and_log_debug()
        {
            var mocks = new MockRepository();
            var logger = mocks.DynamicMock<ILogger>();

            var invocation = mocks.StrictMock<IColomboSendInvocation>();
            var request = new TestRequest();
            var requests = new List<BaseRequest> { request };

            With.Mocks(mocks).Expecting(() =>
            {
                SetupResult.For(invocation.Requests).Return(requests);
                invocation.Proceed();
                logger.DebugFormat(null);
                LastCall.IgnoreArguments();
            }).Verify(() =>
            {
                var interceptor = new SLASendInterceptor();
                interceptor.Logger = logger;
                interceptor.Intercept(invocation);
            });
        }

        [Test]
        public void It_should_log_warn_if_SLA_is_breached()
        {
            var mocks = new MockRepository();
            var logger = mocks.DynamicMock<ILogger>();

            var invocation = mocks.StrictMock<IColomboSendInvocation>();
            var request1 = new TestRequest();
            var request2 = new TestRequestSLA();
            var requests = new List<BaseRequest> { request1, request2 };

            With.Mocks(mocks).Expecting(() =>
            {
                SetupResult.For(invocation.Requests).Return(requests);
                invocation.Proceed();
                LastCall.Do(new ProceedDelegate(() =>
                {
                    Thread.Sleep(200);
                }));
                logger.Warn(null);
                LastCall.IgnoreArguments();
            }).Verify(() =>
            {
                var interceptor = new SLASendInterceptor();
                interceptor.Logger = logger;
                interceptor.Intercept(invocation);
            });
        }

        [Test]
        public void It_should_contact_all_alerters_if_SLA_is_breached()
        {
            var mocks = new MockRepository();
            var logger = mocks.DynamicMock<ILogger>();

            var invocation = mocks.StrictMock<IColomboSendInvocation>();
            var request1 = new TestRequest();
            var request2 = new TestRequestSLA();
            var requests = new List<BaseRequest> { request1, request2 };

            var alerter1 = mocks.StrictMock<IColomboAlerter>();
            var alerter2 = mocks.StrictMock<IColomboAlerter>();

            With.Mocks(mocks).Expecting(() =>
            {
                SetupResult.For(invocation.Requests).Return(requests);
                invocation.Proceed();
                LastCall.Do(new ProceedDelegate(() =>
                {
                    Thread.Sleep(200);
                }));

                alerter1.Alert(null);
                LastCall.IgnoreArguments();
                alerter2.Alert(null);
                LastCall.IgnoreArguments();

            }).Verify(() =>
            {
                var interceptor = new SLASendInterceptor();
                interceptor.Logger = logger;
                interceptor.Alerters = new IColomboAlerter[] { alerter1, alerter2 };
                interceptor.Intercept(invocation);
            });
        }

        public delegate void ProceedDelegate();

        public class TestRequest : Request<TestResponse>
        {
        }

        [SLA(10)]
        public class TestRequestSLA : Request<TestResponse>
        {
        }
    }
}
