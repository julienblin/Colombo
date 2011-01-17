﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Core.Logging;
using Colombo.Alerts;
using Colombo.Interceptors;
using NUnit.Framework;
using Rhino.Mocks;

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
                    Rhino.Mocks.Constraints.Is.TypeOf<ExceptionAlert>()
                );
                alerter2.Alert(null);
                LastCall.IgnoreArguments().Constraints(
                    Rhino.Mocks.Constraints.Is.TypeOf<ExceptionAlert>()
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