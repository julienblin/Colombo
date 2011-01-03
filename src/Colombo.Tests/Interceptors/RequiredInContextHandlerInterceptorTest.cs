using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Colombo.Interceptors;

namespace Colombo.Tests.Interceptors
{
    [TestFixture]
    public class RequiredInContextHandlerInterceptorTest
    {
        [Test]
        public void It_should_proceed_with_the_invocation_when_no_attribute_is_on_request()
        {
            var mocks = new MockRepository();

            var interceptor = new RequiredInContextHandlerInterceptor();
            var invocation = mocks.StrictMock<IColomboInvocation>();
            var request = new TestRequest();

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
        public void It_should_proceed_with_the_invocation_when_keys_are_ok()
        {
            var mocks = new MockRepository();

            var interceptor = new RequiredInContextHandlerInterceptor();
            var invocation = mocks.StrictMock<IColomboInvocation>();
            var request = new TestRequestKeys();
            request.Context["Culture"] = "Culture";
            request.Context["TenandId"] = "TenandId";
            request.Context["UserId"] = "UserId";

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
        public void It_should_throw_an_exception_when_keys_are_missing()
        {
            var mocks = new MockRepository();

            var interceptor = new RequiredInContextHandlerInterceptor();
            var invocation = mocks.StrictMock<IColomboInvocation>();
            var request = new TestRequestKeys();
            request.Context["Culture"] = "Culture";

            With.Mocks(mocks).Expecting(() =>
            {
                SetupResult.For(invocation.Request).Return(request);
            }).Verify(() =>
            {
                Assert.That(() => interceptor.Intercept(invocation),
                    Throws.Exception.TypeOf<RequiredInContextException>()
                    .With.Message.Contains("TenandId")
                    .With.Message.Contains("UserId")
                    .With.Message.Not.Contains("Culture"));
            });
        }

        [Test]
        public void It_should_throw_an_exception_when_keys_are_there_but_empty()
        {
            var mocks = new MockRepository();

            var interceptor = new RequiredInContextHandlerInterceptor();
            var invocation = mocks.StrictMock<IColomboInvocation>();
            var request = new TestRequestKeys();
            request.Context["Culture"] = "Culture";
            request.Context["TenandId"] = "TenandId";
            request.Context["UserId"] = "";

            With.Mocks(mocks).Expecting(() =>
            {
                SetupResult.For(invocation.Request).Return(request);
            }).Verify(() =>
            {
                Assert.That(() => interceptor.Intercept(invocation),
                    Throws.Exception.TypeOf<RequiredInContextException>()
                    .With.Message.Contains("UserId")
                    .With.Message.Not.Contains("Culture")
                    .With.Message.Not.Contains("TenandId"));
            });
        }

        public class TestRequest : Request<TestResponse>
        {
        }

#pragma warning disable 3016 // CLS Compliant
        [RequiredInContext("Culture", "TenandId")]
        [RequiredInContext("UserId", "TenandId")]
        public class TestRequestKeys : Request<TestResponse>
        {
        }
#pragma warning restore 3016
    }
}
