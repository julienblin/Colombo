using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Rhino.Mocks;
using Colombo.Interceptors;
using Colombo.Impl;
using Colombo.Caching;

namespace Colombo.Tests.Interceptors
{
    [TestFixture]
    public class ClientCacheSendInterceptorTest : BaseTest
    {
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

            var cacheFactory = mocks.StrictMock<ICacheFactory>();

            With.Mocks(mocks).Expecting(() =>
            {
                SetupResult.For(invocation.Requests).Return(requests);
                SetupResult.For(invocation.Responses).Return(responses);
                invocation.Proceed();
            }).Verify(() =>
            {
                var interceptor = new ClientCacheSendInterceptor(cacheFactory);
                interceptor.Logger = GetConsoleLogger();
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

            var response1 = new TestResponse();
            var response2 = new TestResponse();
            var responses = new ResponsesGroup
            {
                { request1, response1 },
                { request2, response2 }
            };

            var invocation = mocks.StrictMock<IColomboSendInvocation>();

            var cacheFactory = mocks.StrictMock<ICacheFactory>();

            With.Mocks(mocks).Expecting(() =>
            {
                SetupResult.For(invocation.Requests).Return(requests);
            }).Verify(() =>
            {
                var interceptor = new ClientCacheSendInterceptor(cacheFactory);
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

            var cacheFactory = mocks.StrictMock<ICacheFactory>();
            var cache = mocks.StrictMock<ICache>();

            With.Mocks(mocks).Expecting(() =>
            {
                SetupResult.For(invocation.Requests).Return(requests);
                SetupResult.For(invocation.Responses).Return(responses);
                invocation.Proceed();

                Expect.Call(cacheFactory.GetCacheForSegment(null)).Return(cache).Repeat.Twice();

                Expect.Call(cache.Get<Response>(request1.GetCacheKey())).Return(null);
                cache.Store(request1.GetCacheKey(), response1, new TimeSpan(0, 0, 30));
            }).Verify(() =>
            {
                var interceptor = new ClientCacheSendInterceptor(cacheFactory);
                interceptor.Logger = GetConsoleLogger();
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

            var cacheFactory = mocks.StrictMock<ICacheFactory>();
            var cache = mocks.StrictMock<ICache>();

            With.Mocks(mocks).Expecting(() =>
            {
                SetupResult.For(invocation.Requests).Return(requests);
                SetupResult.For(invocation.Responses).Return(responses).Repeat.Twice();
                invocation.Proceed();

                Expect.Call(cacheFactory.GetCacheForSegment(null)).Return(cache);

                Expect.Call(cache.Get<Response>(request1.GetCacheKey())).Return(response1);
            }).Verify(() =>
            {
                var interceptor = new ClientCacheSendInterceptor(cacheFactory);
                interceptor.Logger = GetConsoleLogger();
                interceptor.Intercept(invocation);
                var responsesVerify = invocation.Responses;
                Assert.That(() => responsesVerify[request1],
                    Is.EqualTo(response1));
                Assert.That(() => responsesVerify[request2],
                    Is.EqualTo(response2));
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

            var cacheFactory = mocks.StrictMock<ICacheFactory>();
            var cache = mocks.StrictMock<ICache>();

            With.Mocks(mocks).Expecting(() =>
            {
                SetupResult.For(invocation.Requests).Return(requests);
                SetupResult.For(invocation.Responses).Return(responses);
                invocation.Proceed();

                Expect.Call(cacheFactory.GetCacheForSegment(null)).Return(cache).Repeat.Times(3);

                Expect.Call(cache.Get<Response>(request1.GetCacheKey())).Return(null);
                cache.InvalidateAllObjects(typeof(TestResponse));
                cache.Store(request1.GetCacheKey(), response1, new TimeSpan(0, 0, 30));
            }).Verify(() =>
            {
                var interceptor = new ClientCacheSendInterceptor(cacheFactory);
                interceptor.Logger = GetConsoleLogger();
                interceptor.Intercept(invocation);
            });
        }

        public class TestRequestWithoutCache : Request<TestResponse> { }

        [EnableClientCaching(Seconds=30)]
        public class TestRequestWithCacheNoCacheKey : Request<TestResponse> { }

        [EnableClientCaching(Seconds=30)]
        public class TestRequestWithCache : SideEffectFreeRequest<TestResponse>
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
