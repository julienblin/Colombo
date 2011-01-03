using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Castle.Core.Logging;
using Colombo.Interceptors;
using System.Threading;

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

            var invocation = mocks.StrictMock<IColomboInvocation>();
            var request = new TestRequest();

            With.Mocks(mocks).Expecting(() =>
            {
                SetupResult.For(invocation.Request).Return(request);
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

            var invocation = mocks.StrictMock<IColomboInvocation>();
            var request = new TestRequestSLA();

            With.Mocks(mocks).Expecting(() =>
            {
                SetupResult.For(invocation.Request).Return(request);
                invocation.Proceed();
                LastCall.Do(new ProceedDelegate(() =>
                {
                    Thread.Sleep(15);
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

            var invocation = mocks.StrictMock<IColomboInvocation>();
            var request = new TestRequestSLA();

            var alerter1 = mocks.StrictMock<IColomboAlerter>();
            var alerter2 = mocks.StrictMock<IColomboAlerter>();

            With.Mocks(mocks).Expecting(() =>
            {
                SetupResult.For(invocation.Request).Return(request);
                invocation.Proceed();
                LastCall.Do(new ProceedDelegate(() =>
                {
                    Thread.Sleep(15);
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
