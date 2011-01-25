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

using System.Globalization;
using System.Threading;
using Colombo.Interceptors;
using NUnit.Framework;
using Rhino.Mocks;

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

            var invocation = mocks.StrictMock<IColomboRequestHandleInvocation>();

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

            var invocation = mocks.StrictMock<IColomboRequestHandleInvocation>();

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
