using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using Colombo.Interceptors;
using NUnit.Framework;
using Rhino.Mocks;

namespace Colombo.Tests.Interceptors
{
    [TestFixture]
    public class CurrentCultureSendInterceptorTest
    {
        [Test]
        public void It_should_positionned_the_CurrentUICulture_into_Context_Culture()
        {
            var mocks = new MockRepository();
            var request1 = new TestRequest();
            var request2 = new TestRequest();
            var requests = new List<BaseRequest> { request1, request2 };

            var invocation = mocks.StrictMock<IColomboSendInvocation>();

            With.Mocks(mocks).Expecting(() =>
            {
                SetupResult.For(invocation.Requests).Return(requests);
                invocation.Proceed();
                invocation.Proceed();
                invocation.Proceed();
            }).Verify(() =>
            {
                var interceptor = new CurrentCultureSendInterceptor();

                Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
                interceptor.Intercept(invocation);
                Assert.That(() => request1.Context.ContainsKey(CurrentCultureConstant.CultureContextKey),
                    Is.False);
                Assert.That(() => request2.Context.ContainsKey(CurrentCultureConstant.CultureContextKey),
                    Is.False);

                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("ar-LB");
                interceptor.Intercept(invocation);
                Assert.That(() => request1.Context[CurrentCultureConstant.CultureContextKey],
                    Is.EqualTo("ar-LB"));
                Assert.That(() => request2.Context[CurrentCultureConstant.CultureContextKey],
                    Is.EqualTo("ar-LB"));

                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-ZW");
                interceptor.Intercept(invocation);
                Assert.That(() => request1.Context[CurrentCultureConstant.CultureContextKey],
                    Is.EqualTo("en-ZW"));
                Assert.That(() => request2.Context[CurrentCultureConstant.CultureContextKey],
                    Is.EqualTo("en-ZW"));
            });
        }

        public class TestRequest : Request<TestResponse>
        {
        }
    }
}
