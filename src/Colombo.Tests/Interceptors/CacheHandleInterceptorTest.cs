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
        public void It_should_do_nothing_when_InvalidateCachedInstancesOf_or_EnableCache_is_not_present()
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
                var interceptor = new CacheHandleInterceptor(cache) { Logger = GetConsoleLogger() };
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
                var interceptor = new CacheHandleInterceptor(cache) { Logger = GetConsoleLogger() };
                interceptor.Intercept(invocation);
            });
        }

        [Test]
        public void It_should_throw_an_exception_if_requests_does_not_implement_GetCacheKey()
        {
            var mocks = new MockRepository();

            var request = new TestRequestCacheWithoutGetCacheKey();
            var response = new TestResponse();
            var invocation = mocks.StrictMock<IColomboRequestHandleInvocation>();
            var cache = mocks.StrictMock<IColomboCache>();

            With.Mocks(mocks).Expecting(() =>
            {
                SetupResult.For(invocation.Request).Return(request);
                SetupResult.For(invocation.Response).Return(response);
                invocation.Proceed();
            }).Verify(() =>
            {
                var interceptor = new CacheHandleInterceptor(cache) { Logger = GetConsoleLogger() };
                Assert.That(() => interceptor.Intercept(invocation),
                    Throws.Exception.TypeOf<ColomboException>()
                    .With.Message.Contains("GetCacheKey"));
            });
        }

        [Test]
        public void It_should_put_in_cache_request_that_enables_it()
        {
            var mocks = new MockRepository();

            var request = new TestRequestCache();
            var response = new TestResponse();
            var invocation = mocks.StrictMock<IColomboRequestHandleInvocation>();
            var cache = mocks.StrictMock<IColomboCache>();

            With.Mocks(mocks).Expecting(() =>
            {
                SetupResult.For(invocation.Request).Return(request);
                SetupResult.For(invocation.Response).Return(response);
                invocation.Proceed();
                cache.Store(null, request.GetCacheKey(), response, new TimeSpan(1, 0, 0));
            }).Verify(() =>
            {
                var interceptor = new CacheHandleInterceptor(cache) { Logger = GetConsoleLogger() };
                interceptor.Intercept(invocation);
            });
        }

        [Test]
        public void It_should_put_in_the_right_cache_segment()
        {
            var mocks = new MockRepository();

            var request = new TestRequestCacheWithSegment();
            request.Context["Foo"] = "Bar";
            var response = new TestResponse();
            var invocation = mocks.StrictMock<IColomboRequestHandleInvocation>();
            var cache = mocks.StrictMock<IColomboCache>();

            With.Mocks(mocks).Expecting(() =>
            {
                SetupResult.For(invocation.Request).Return(request);
                SetupResult.For(invocation.Response).Return(response);
                invocation.Proceed();
                cache.Store("Bar", request.GetCacheKey(), response, new TimeSpan(0, 1, 0));
            }).Verify(() =>
            {
                var interceptor = new CacheHandleInterceptor(cache) { Logger = GetConsoleLogger() };
                interceptor.Intercept(invocation);
            });
        }

        public class TestRequest : Request<TestResponse> { }

        public class TestResponse2 : Response { }

#pragma warning disable 3016 // CLS Compliant
        
        [InvalidateCachedInstancesOf(typeof(TestResponse), typeof(TestResponse2))]
        [CacheSegment(Name = "CacheSegment")]
        public class TestRequestInvalidate : Request<TestResponse>
        {
        }

        [EnableCache(Hours = 1)]
        [CacheSegment(Name = "CacheSegment")]
        public class TestRequestCacheWithoutGetCacheKey : Request<TestResponse>
        {
        }

        [EnableCache(Hours = 1)]
        public class TestRequestCache : Request<TestResponse>
        {
            public override string GetCacheKey()
            {
                return GetType().Name;
            }
        }

        [EnableCache(Minutes = 1)]
        [CacheSegment(FromContextKey = "Foo")]
        public class TestRequestCacheWithSegment : Request<TestResponse>
        {
            public override string GetCacheKey()
            {
                return GetType().Name;
            }
        }

#pragma warning restore 3016
    }
}
