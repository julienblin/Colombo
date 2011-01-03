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
    public class CurrentCultureHandlerInterceptorTest
    {
        [Test]
        public void It_should_positionned_the_CurrentCulture_and_CurrentUICulture_from_CallContext_Culture()
        {
            var mocks = new MockRepository();
            var request = mocks.Stub<Request<TestResponse>>();
            request.CallContext = new CallContext();

            var invocation = mocks.StrictMock<IColomboInvocation>();

            With.Mocks(mocks).Expecting(() =>
            {
                SetupResult.For(invocation.Request).Return(request);
                invocation.Proceed();
                invocation.Proceed();
            }).Verify(() =>
            {
                var interceptor = new CurrentCultureHandlerInterceptor();

                request.CallContext.Culture = "ar-LB";
                interceptor.Intercept(invocation);
                Assert.That(() => Thread.CurrentThread.CurrentUICulture,
                    Is.EqualTo(CultureInfo.GetCultureInfo("ar-LB")));

                request.CallContext.Culture = "en-ZW";
                interceptor.Intercept(invocation);
                Assert.That(() => Thread.CurrentThread.CurrentUICulture,
                    Is.EqualTo(CultureInfo.GetCultureInfo("en-ZW")));
            });
        }
    }
}
