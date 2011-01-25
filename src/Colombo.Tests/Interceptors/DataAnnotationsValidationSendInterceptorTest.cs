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

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Colombo.Impl.Send;
using Colombo.Interceptors;
using NUnit.Framework;
using Rhino.Mocks;

namespace Colombo.Tests.Interceptors
{
    [TestFixture]
    public class DataAnnotationsValidationSendInterceptorTest
    {
        [Test]
        public void It_should_proceed_with_the_invocation_when_validation_pass()
        {
            var mocks = new MockRepository();

            var interceptor = new DataAnnotationsValidationSendInterceptor();
            var invocation = mocks.StrictMock<IColomboSendInvocation>();
            var request1 = new TestRequest();
            request1.FirstName = "FirstName";
            request1.LastName = "LastName";

            var request2 = new TestRequestWithResults();
            request2.FirstName = "FirstName";
            request2.LastName = "LastName";

            var requests = new List<BaseRequest> { request1, request2 };

            With.Mocks(mocks).Expecting(() =>
            {
                SetupResult.For(invocation.Requests).Return(requests);
                invocation.Proceed();
            }).Verify(() =>
            {
                interceptor.Intercept(invocation);
            });
        }

        [Test]
        public void It_should_throw_an_exception_when_validation_fail_and_not_ValidatedResponse()
        {
            var mocks = new MockRepository();

            var interceptor = new DataAnnotationsValidationSendInterceptor();
            var invocation = mocks.StrictMock<IColomboSendInvocation>();
            var request1 = new TestRequest();
            request1.FirstName = "FirstName";

            var request2 = new TestRequestWithResults();
            request2.FirstName = "FirstName";
            request2.LastName = "LastName";

            var requests = new List<BaseRequest> { request1, request2 };

            With.Mocks(mocks).Expecting(() =>
            {
                SetupResult.For(invocation.Requests).Return(requests);
            }).Verify(() =>
            {
                Assert.That(() => interceptor.Intercept(invocation),
                    Throws.Exception.TypeOf<ValidationException>());
            });
        }

        [Test]
        public void It_should_populate_ValidationResults_when_validation_fail_and_ValidatedResponse()
        {
            var interceptor = new DataAnnotationsValidationSendInterceptor();
            var invocation = new TestHandleInvocation();
            var request1 = new TestRequest();
            request1.FirstName = "FirstName";
            request1.LastName = "LastName";

            var request2 = new TestRequestWithResults();

            invocation.Requests = new List<BaseRequest> { request1, request2 };

            interceptor.Intercept(invocation);

            var response2 = invocation.Responses.GetFrom(request2);

            Assert.That(() => response2,
                Is.TypeOf<TestResponseWithResults>());

            Assert.That(() => response2.CorrelationGuid,
                    Is.EqualTo(request2.CorrelationGuid));
            Assert.That(() => response2.ValidationResults.Count,
                Is.EqualTo(2));
            Assert.That(() => response2.ValidationResults[0].MemberNames.First(),
                Is.EqualTo("FirstName"));
            Assert.That(() => response2.ValidationResults[1].MemberNames.First(),
                Is.EqualTo("LastName"));
        }

        internal class TestHandleInvocation : BaseSendInvocation
        {
            public override void Proceed()
            {
                
            }
        }

        public class TestRequest : SideEffectFreeRequest<TestResponse>
        {
            [Required(AllowEmptyStrings = false)]
            public string FirstName { get; set; }

            [Required(AllowEmptyStrings = false)]
            public string LastName { get; set; }
        }

        public class TestResponseWithResults : ValidatedResponse
        {
        }

        public class TestRequestWithResults : SideEffectFreeRequest<TestResponseWithResults>
        {
            [Required(AllowEmptyStrings = false)]
            public string FirstName { get; set; }

            [Required(AllowEmptyStrings = false)]
            public string LastName { get; set; }
        }
    }
}
