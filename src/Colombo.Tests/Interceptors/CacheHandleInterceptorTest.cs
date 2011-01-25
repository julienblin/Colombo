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

using System;
using Colombo.Caching;
using Colombo.Interceptors;
using NUnit.Framework;
using Rhino.Mocks;

namespace Colombo.Tests.Interceptors
{
    [TestFixture]
    public class CacheHandleInterceptorTest : BaseTest
    {
        [Test]
        public void It_should_ensure_that_it_has_an_ICacheFactory()
        {
            Assert.That(() => new CacheHandleInterceptor(null),
                Throws.Exception.TypeOf<ArgumentNullException>()
                .With.Message.Contains("cache"));
        }

        [Test]
        public void It_should_do_nothing_when_InvalidateCachedInstancesOf_is_not_present()
        {
            var mocks = new MockRepository();

            var request = new TestRequest();
            var invocation = mocks.StrictMock<IColomboRequestHandleInvocation>();
            var cache = mocks.StrictMock<IColomboCache>();

            With.Mocks(mocks).Expecting(() =>
            {
                SetupResult.For(invocation.Request).Return(request);
                invocation.Proceed();
            }).Verify(() =>
            {
                var interceptor = new CacheHandleInterceptor(cache);
                interceptor.Logger = GetConsoleLogger();
                interceptor.Intercept(invocation);
            });
        }

        [Test]
        public void It_should_invalidate_cache_when_InvalidateCachedInstancesOf()
        {
            var mocks = new MockRepository();

            var request = new TestRequestInvalidate();
            var invocation = mocks.StrictMock<IColomboRequestHandleInvocation>();
            var cache = mocks.StrictMock<IColomboCache>();

            With.Mocks(mocks).Expecting(() =>
            {
                SetupResult.For(invocation.Request).Return(request);
                invocation.Proceed();

                cache.Flush("CacheSegment", typeof(TestResponse));
                cache.Flush("CacheSegment", typeof(TestResponse2));
            }).Verify(() =>
            {
                var interceptor = new CacheHandleInterceptor(cache);
                interceptor.Logger = GetConsoleLogger();
                interceptor.Intercept(invocation);
            });
        }

        public class TestRequest : Request<TestResponse> { }

        public class TestResponse2 : Response { }

#pragma warning disable 3016 // CLS Compliant
        [InvalidateCachedInstancesOf(typeof(TestResponse), typeof(TestResponse2))]
#pragma warning restore 3016
        [CacheSegment(Name="CacheSegment")]
        public class TestRequestInvalidate : Request<TestResponse>
        {
        }
    }
}
