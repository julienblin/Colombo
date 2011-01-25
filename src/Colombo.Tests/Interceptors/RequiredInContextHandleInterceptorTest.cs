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

using Colombo.Interceptors;
using NUnit.Framework;
using Rhino.Mocks;

namespace Colombo.Tests.Interceptors
{
    [TestFixture]
    public class RequiredInContextHandleInterceptorTest
    {
        [Test]
        public void It_should_proceed_with_the_invocation_when_no_attribute_is_on_request()
        {
            var mocks = new MockRepository();

            var interceptor = new RequiredInContextHandleInterceptor();
            var invocation = mocks.StrictMock<IColomboRequestHandleInvocation>();
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

            var interceptor = new RequiredInContextHandleInterceptor();
            var invocation = mocks.StrictMock<IColomboRequestHandleInvocation>();
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

            var interceptor = new RequiredInContextHandleInterceptor();
            var invocation = mocks.StrictMock<IColomboRequestHandleInvocation>();
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

            var interceptor = new RequiredInContextHandleInterceptor();
            var invocation = mocks.StrictMock<IColomboRequestHandleInvocation>();
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
