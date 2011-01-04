using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.ComponentModel.DataAnnotations;
using Rhino.Mocks;
using Colombo.Interceptors;

namespace Colombo.Tests.Interceptors
{
    [TestFixture]
    public class DataAnnotationInterceptorTest
    {
        [Test]
        public void It_should_proceed_with_the_invocation_when_validation_pass()
        {
            var mocks = new MockRepository();

            var interceptor = mocks.Stub<DataAnnotationInterceptor>();
            var invocation = mocks.StrictMock<IColomboSingleInvocation>();
            var request = new TestRequest();
            request.Name = "Name";

            With.Mocks(mocks).Expecting(() =>
            {
                SetupResult.For(invocation.Request).Return(request);
                invocation.Proceed();
            }).Verify(() =>
            {
                interceptor.Intercept(invocation);
            });
        }

        [Test]
        public void It_should_not_proceed_with_the_invocation_when_validation_fail()
        {
            var mocks = new MockRepository();

            var request = new TestRequest();
            request.Name = "";
            var invocation = new TestInvocation(request);

            var interceptor = mocks.Stub<DataAnnotationInterceptor>();
            interceptor.Intercept(invocation);

            Assert.That(() => invocation.ProceedCalled,
                Is.False);
        }

        [Test]
        public void It_should_create_appropriate_response_and_validation_results_when_validation_fail()
        {
            var mocks = new MockRepository();

            var request = new TestRequest();
            request.Name = "";
            var invocation = new TestInvocation(request);

            var interceptor = mocks.Stub<DataAnnotationInterceptor>();
            interceptor.Intercept(invocation);

            Assert.That(() => invocation.Response,
                Is.TypeOf<TestResponse>());
            Assert.That(() => invocation.Response.CorrelationGuid,
                Is.EqualTo(request.CorrelationGuid));
            Assert.That(() => invocation.Response.ValidationResults.Count,
                Is.EqualTo(1));
        }

        public class TestRequest : Request<TestResponse>
        {
            [Required(AllowEmptyStrings = false)]
            public string Name { get; set; }
        }

        public class TestInvocation : IColomboSingleInvocation
        {
            public TestInvocation(BaseRequest request)
            {
                Request = request;
                ProceedCalled = false;
            }

            public BaseRequest Request { get; private set; }

            public Response Response { get; set; }

            public bool ProceedCalled { get; set; }
            public void Proceed()
            {
                ProceedCalled = true;
            }
        }

    }
}
