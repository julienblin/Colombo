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
using System.Collections.Generic;
using Colombo.Caching;
using Colombo.Interceptors;
using NUnit.Framework;
using Rhino.Mocks;

namespace Colombo.Tests.Interceptors
{
    [TestFixture]
    public class ClientCacheSendInterceptorTest : BaseTest
    {
        [Test]
        public void It_should_ensure_that_it_has_an_ICacheFactory()
        {
            Assert.That(() => new ClientCacheSendInterceptor(null),
                Throws.Exception.TypeOf<ArgumentNullException>()
                .With.Message.Contains("cache"));
        }

        [Test]
        public void It_should_ignore_requests_not_marked_with_cache()
        {
            var mocks = new MockRepository();

            var request1 = new TestRequestWithoutCache();
            var request2 = new TestRequestWithoutCache();
            var requests = new List<BaseRequest> { request1, request2 };

            var response1 = new TestResponse();
            var response2 = new TestResponse();
            var responses = new ResponsesGroup
            {
                { request1, response1 },
                { request2, response2 }
            };

            var invocation = mocks.StrictMock<IColomboSendInvocation>();

            var cache = mocks.StrictMock<IColomboCache>();

            With.Mocks(mocks).Expecting(() =>
            {
                SetupResult.For(invocation.Requests).Return(requests);
                SetupResult.For(invocation.Responses).Return(responses);
                invocation.Proceed();
            }).Verify(() =>
            {
                var interceptor = new ClientCacheSendInterceptor(cache) { Logger = GetConsoleLogger() };
                interceptor.Intercept(invocation);
            });
        }

        [Test]
        public void It_should_throw_an_exception_if_requests_does_not_implement_GetCacheKey()
        {
            var mocks = new MockRepository();

            var request1 = new TestRequestWithCacheNoCacheKey();
            var request2 = new TestRequestWithoutCache();
            var requests = new List<BaseRequest> { request1, request2 };

            var invocation = mocks.StrictMock<IColomboSendInvocation>();
            var cache = mocks.StrictMock<IColomboCache>();

            With.Mocks(mocks).Expecting(() =>
            {
                SetupResult.For(invocation.Requests).Return(requests);
            }).Verify(() =>
            {
                var interceptor = new ClientCacheSendInterceptor(cache);
                interceptor.Logger = GetConsoleLogger();
                Assert.That(() => interceptor.Intercept(invocation),
                    Throws.Exception.TypeOf<ColomboException>()
                    .With.Message.Contains("GetCacheKey"));
            });
        }

        [Test]
        public void It_should_put_in_cache_requests_that_enables_it()
        {
            var mocks = new MockRepository();

            var request1 = new TestRequestWithCache();
            request1.Context["Foo"] = "Bar";
            var request2 = new TestRequestWithoutCache();
            var requests = new List<BaseRequest> { request1, request2 };

            var response1 = new TestResponse();
            var response2 = new TestResponse();
            var responses = new ResponsesGroup
            {
                { request1, response1 },
                { request2, response2 }
            };

            var invocation = mocks.StrictMock<IColomboSendInvocation>();
            var cache = mocks.StrictMock<IColomboCache>();

            With.Mocks(mocks).Expecting(() =>
            {
                SetupResult.For(invocation.Requests).Return(requests);
                SetupResult.For(invocation.Responses).Return(responses);
                invocation.Proceed();

                Expect.Call(cache.Get(null, typeof(TestResponse), request1.GetCacheKey())).Return(null);
                cache.Store(null, request1.GetCacheKey(), response1, new TimeSpan(0, 0, 30));
            }).Verify(() =>
            {
                var interceptor = new ClientCacheSendInterceptor(cache) { Logger = GetConsoleLogger() };
                interceptor.Intercept(invocation);
            });
        }

        [Test]
        public void It_should_retrieve_cached_requests()
        {
            var mocks = new MockRepository();

            var request1 = new TestRequestWithCache();
            request1.Context["Foo"] = "Bar";
            var request2 = new TestRequestWithoutCache();
            var requests = new List<BaseRequest> { request1, request2 };

            var response1 = new TestResponse();
            var response2 = new TestResponse();
            var responses = new ResponsesGroup
            {
                { request2, response2 }
            };

            var invocation = mocks.StrictMock<IColomboSendInvocation>();
            var cache = mocks.StrictMock<IColomboCache>();

            With.Mocks(mocks).Expecting(() =>
            {
                SetupResult.For(invocation.Requests).Return(requests);
                SetupResult.For(invocation.Responses).Return(responses).Repeat.Twice();
                invocation.Proceed();

                Expect.Call(cache.Get(null, typeof(TestResponse), request1.GetCacheKey())).Return(response1);
            }).Verify(() =>
            {
                var interceptor = new ClientCacheSendInterceptor(cache) { Logger = GetConsoleLogger() };
                interceptor.Intercept(invocation);
                var responsesVerify = invocation.Responses;
                Assert.That(responsesVerify[request1], Is.EqualTo(response1));
                Assert.That(responsesVerify[request2], Is.EqualTo(response2));
            });
        }

        [Test]
        public void It_should_select_appropriate_cache_segment()
        {
            var mocks = new MockRepository();

            var request1 = new TestRequestWithCacheSegment();
            request1.Context["Foo"] = "Request1";
            var request2 = new TestRequestWithCacheSegment();
            request2.Context["Foo"] = "Request2";
            request2.Context["Bar"] = "CacheSegmentFromContext";
            var requests = new List<BaseRequest> { request1, request2 };

            var response1 = new TestResponse();
            var response2 = new TestResponse();
            var responses = new ResponsesGroup();

            var invocation = mocks.StrictMock<IColomboSendInvocation>();
            var cache = mocks.StrictMock<IColomboCache>();

            With.Mocks(mocks).Expecting(() =>
            {
                SetupResult.For(invocation.Requests).Return(requests);
                SetupResult.For(invocation.Responses).Return(responses).Repeat.Twice();
                invocation.Proceed();

                Expect.Call(cache.Get("DefaultSegment", typeof(TestResponse), request1.GetCacheKey())).Return(response1);
                Expect.Call(cache.Get("CacheSegmentFromContext", typeof(TestResponse), request2.GetCacheKey())).Return(response2);
            }).Verify(() =>
            {
                var interceptor = new ClientCacheSendInterceptor(cache);
                interceptor.Logger = GetConsoleLogger();
                interceptor.Intercept(invocation);
                var responsesVerify = invocation.Responses;
                Assert.That(responsesVerify[request1], Is.EqualTo(response1));
                Assert.That(responsesVerify[request2], Is.EqualTo(response2));
            });
        }

        [Test]
        public void It_should_invalidate_responses_when_request_comes_in_with_InvalidateCachedInstancesOf()
        {
            var mocks = new MockRepository();

            var request1 = new TestRequestInvalidate();
            var requests = new List<BaseRequest> { request1 };

            var response1 = new TestResponse();
            var responses = new ResponsesGroup
            {
                { request1, response1 }
            };

            var invocation = mocks.StrictMock<IColomboSendInvocation>();
            var cache = mocks.StrictMock<IColomboCache>();

            With.Mocks(mocks).Expecting(() =>
            {
                SetupResult.For(invocation.Requests).Return(requests);
                SetupResult.For(invocation.Responses).Return(responses);
                invocation.Proceed();

                Expect.Call(cache.Get(null, typeof(TestResponse), request1.GetCacheKey())).Return(null);
                cache.Flush(null, typeof(TestResponse));
                cache.Store(null, request1.GetCacheKey(), response1, new TimeSpan(0, 0, 30));
            }).Verify(() =>
            {
                var interceptor = new ClientCacheSendInterceptor(cache) { Logger = GetConsoleLogger() };
                interceptor.Intercept(invocation);
            });
        }

        public class TestRequestWithoutCache : Request<TestResponse> { }

        [EnableClientCaching(Seconds = 30)]
        public class TestRequestWithCacheNoCacheKey : Request<TestResponse> { }

        [EnableClientCaching(Seconds = 30)]
        public class TestRequestWithCache : SideEffectFreeRequest<TestResponse>
        {
            public override string GetCacheKey()
            {
                return string.Format("{0}+{1}", GetType().Name, Context["Foo"]);
            }
        }

        [EnableClientCaching(Seconds = 30)]
        [CacheSegment(FromContextKey = "Bar", Name = "DefaultSegment")]
        public class TestRequestWithCacheSegment : Request<TestResponse>
        {
            public override string GetCacheKey()
            {
                return string.Format("{0}+{1}", GetType().Name, Context["Foo"]);
            }
        }

        [EnableClientCaching(Seconds = 30)]
        [InvalidateCachedInstancesOf(typeof(TestResponse))]
        public class TestRequestInvalidate : Request<TestResponse>
        {
            public override string GetCacheKey()
            {
                return GetType().Name;
            }
        }
    }
}
