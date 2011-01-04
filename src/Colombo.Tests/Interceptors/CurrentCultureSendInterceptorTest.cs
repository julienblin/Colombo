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
    public class CurrentCultureSendInterceptorTest
    {
        [Test]
        public void It_should_positionned_the_CurrentUICulture_into_CallContext_Culture()
        {
            var mocks = new MockRepository();
            var request = mocks.Stub<Request<TestResponse>>();

            var invocation = mocks.StrictMock<IColomboSingleInvocation>();

            With.Mocks(mocks).Expecting(() =>
            {
                SetupResult.For(invocation.Request).Return(request);
                invocation.Proceed();
                invocation.Proceed();
            }).Verify(() =>
            {
                var interceptor = new CurrentCultureSendInterceptor();

                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("ar-LB");
                interceptor.Intercept(invocation);
                Assert.That(() => request.Context[CurrentCultureConstant.CultureContextKey],
                    Is.EqualTo("ar-LB"));

                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-ZW");
                interceptor.Intercept(invocation);
                Assert.That(() => request.Context[CurrentCultureConstant.CultureContextKey],
                    Is.EqualTo("en-ZW"));
            });
        }
    }
}
