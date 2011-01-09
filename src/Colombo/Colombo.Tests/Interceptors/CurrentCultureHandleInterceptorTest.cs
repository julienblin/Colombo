using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Colombo.Interceptors;
using System.Threading;
using System.Globalization;

namespace Colombo.Tests.Interceptors
{
    [TestFixture]
    public class CurrentCultureHandleInterceptorTest
    {
        [Test]
        public void It_should_positionned_the_CurrentCulture_and_CurrentUICulture_from_CallContext_Culture()
        {
            var mocks = new MockRepository();
            var request = new TestRequest();

            var invocation = mocks.StrictMock<IColomboHandleInvocation>();

            With.Mocks(mocks).Expecting(() =>
            {
                SetupResult.For(invocation.Request).Return(request);
                invocation.Proceed();
                invocation.Proceed();
            }).Verify(() =>
            {
                var interceptor = new CurrentCultureHandleInterceptor();

                request.Context[CurrentCultureConstant.CultureContextKey] = "ar-LB";
                interceptor.Intercept(invocation);
                Assert.That(() => Thread.CurrentThread.CurrentUICulture,
                    Is.EqualTo(CultureInfo.GetCultureInfo("ar-LB")));

                request.Context[CurrentCultureConstant.CultureContextKey] = "en-ZW";
                interceptor.Intercept(invocation);
                Assert.That(() => Thread.CurrentThread.CurrentUICulture,
                    Is.EqualTo(CultureInfo.GetCultureInfo("en-ZW")));
            });
        }

        [Test]
        public void It_should_position_an_invariant_Culture_when_no_key_is_present()
        {
            var mocks = new MockRepository();
            var request = new TestRequest();

            var invocation = mocks.StrictMock<IColomboHandleInvocation>();

            With.Mocks(mocks).Expecting(() =>
            {
                SetupResult.For(invocation.Request).Return(request);
                invocation.Proceed();
            }).Verify(() =>
            {
                var interceptor = new CurrentCultureHandleInterceptor();

                interceptor.Intercept(invocation);
                Assert.That(() => Thread.CurrentThread.CurrentUICulture,
                    Is.EqualTo(CultureInfo.InvariantCulture));
            });
        }

        public class TestRequest : Request<TestResponse>
        {
        }
    }
}
