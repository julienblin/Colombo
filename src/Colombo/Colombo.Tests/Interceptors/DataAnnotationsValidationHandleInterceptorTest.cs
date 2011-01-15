using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Colombo.Interceptors;
using System.ComponentModel.DataAnnotations;
using Colombo.Impl;
using Colombo.Impl.RequestHandle;

namespace Colombo.Tests.Interceptors
{
    [TestFixture]
    public class DataAnnotationsValidationHandleInterceptorTest
    {
        [Test]
        public void It_should_proceed_with_the_invocation_when_validation_pass()
        {
            var mocks = new MockRepository();

            var interceptor = new DataAnnotationsValidationHandleInterceptor();
            var invocation1 = mocks.StrictMock<IColomboRequestHandleInvocation>();
            var invocation2 = mocks.StrictMock<IColomboRequestHandleInvocation>();
            var request1 = new TestRequest();
            request1.FirstName = "FirstName";
            request1.LastName = "LastName";

            var request2 = new TestRequestWithResults();
            request2.FirstName = "FirstName";
            request2.LastName = "LastName";

            With.Mocks(mocks).Expecting(() =>
            {
                SetupResult.For(invocation1.Request).Return(request1);
                invocation1.Proceed();

                SetupResult.For(invocation2.Request).Return(request1);
                invocation2.Proceed();
            }).Verify(() =>
            {
                interceptor.Intercept(invocation1);
                interceptor.Intercept(invocation2);
            });
        }

        [Test]
        public void It_should_throw_an_exception_when_validation_fail_and_not_ValidatedResponse()
        {
            var mocks = new MockRepository();

            var interceptor = new DataAnnotationsValidationHandleInterceptor();
            var invocation = mocks.StrictMock<IColomboRequestHandleInvocation>();
            var request = new TestRequest();

            With.Mocks(mocks).Expecting(() =>
            {
                SetupResult.For(invocation.Request).Return(request);
            }).Verify(() =>
            {
                Assert.That(() => interceptor.Intercept(invocation),
                    Throws.Exception.TypeOf<ValidationException>());
            });
        }

        [Test]
        public void It_should_populate_ValidationResults_when_validation_fail_and_ValidatedResponse()
        {
            var interceptor = new DataAnnotationsValidationHandleInterceptor();
            var invocation = new TestHandleInvocation();
            var request = new TestRequestWithResults();
            invocation.Request = request;

            interceptor.Intercept(invocation);

            Assert.That(() => invocation.Response,
                Is.TypeOf<TestResponseWithResults>());
            Assert.That(() => ((TestResponseWithResults)invocation.Response).CorrelationGuid,
                    Is.EqualTo(request.CorrelationGuid));
            Assert.That(() => ((TestResponseWithResults)invocation.Response).ValidationResults.Count,
                Is.EqualTo(2));
            Assert.That(() => ((TestResponseWithResults)invocation.Response).ValidationResults[0].MemberNames.First(),
                Is.EqualTo("FirstName"));
            Assert.That(() => ((TestResponseWithResults)invocation.Response).ValidationResults[1].MemberNames.First(),
                Is.EqualTo("LastName"));
        }

        internal class TestHandleInvocation : BaseRequestHandleInvocation
        {
            public override void Proceed()
            {
                throw new NotImplementedException();
            }
        }

        public class TestRequest : Request<TestResponse>
        {
            [Required(AllowEmptyStrings = false)]
            public string FirstName { get; set; }

            [Required(AllowEmptyStrings = false)]
            public string LastName { get; set; }
        }

        public class TestResponseWithResults : ValidatedResponse
        {
        }

        public class TestRequestWithResults : Request<TestResponseWithResults>
        {
            [Required(AllowEmptyStrings = false)]
            public string FirstName { get; set; }

            [Required(AllowEmptyStrings = false)]
            public string LastName { get; set; }
        }
    }
}
