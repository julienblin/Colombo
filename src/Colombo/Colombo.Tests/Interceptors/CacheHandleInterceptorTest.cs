using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Colombo.Interceptors;
using Rhino.Mocks;
using Colombo.Caching;

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
