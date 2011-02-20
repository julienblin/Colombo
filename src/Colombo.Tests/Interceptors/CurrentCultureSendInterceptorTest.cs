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
                Assert.That(request1.Context.ContainsKey(CurrentCultureConstant.CultureContextKey), Is.False);
                Assert.That(request2.Context.ContainsKey(CurrentCultureConstant.CultureContextKey), Is.False);

                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("ar-LB");
                interceptor.Intercept(invocation);
                Assert.That(request1.Context[CurrentCultureConstant.CultureContextKey], Is.EqualTo("ar-LB"));
                Assert.That(request2.Context[CurrentCultureConstant.CultureContextKey], Is.EqualTo("ar-LB"));

                Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("en-ZW");
                interceptor.Intercept(invocation);
                Assert.That(request1.Context[CurrentCultureConstant.CultureContextKey], Is.EqualTo("en-ZW"));
                Assert.That(request2.Context[CurrentCultureConstant.CultureContextKey], Is.EqualTo("en-ZW"));
            });
        }

        public class TestRequest : Request<TestResponse>
        {
        }
    }
}
