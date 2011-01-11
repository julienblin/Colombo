using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;
using System.Diagnostics;
using Colombo.Caching.Impl;
using System.Threading;

namespace Colombo.Tests.Caching.Impl
{
    [TestFixture]
    public class MemcachedCacheTest : BaseTest
    {
        const string serverUri = "localhost";
        Process memcachedServerProcess;

        [SetUp]
        public void SetUp()
        {
            var memcachedServerPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "memcached.exe");
            memcachedServerProcess = Process.Start(memcachedServerPath);
        }

        [Test]
        public void It_should_throw_an_exception_when_cacheKey_or_object_is_null_or_duration_is_max()
        {
            var cache = new MemcachedCache(serverUri);

            Assert.That(() => cache.Store(null, "", new Object(), new TimeSpan(0, 0, 1)),
                Throws.Exception.TypeOf<ArgumentNullException>()
                .With.Message.Contains("cacheKey"));

            Assert.That(() => cache.Store(null, "key", null, new TimeSpan(0, 0, 1)),
                Throws.Exception.TypeOf<ArgumentNullException>()
                .With.Message.Contains("object"));

            Assert.That(() => cache.Store(null, "key", new Object(), TimeSpan.MaxValue),
                Throws.Exception.TypeOf<ArgumentException>()
                .With.Message.Contains("duration"));

            Assert.DoesNotThrow(() => cache.Store(null, "key", new Object(), new TimeSpan(0, 0, 1)));
        }

        [Test]
        public void It_should_store_and_retrieve_objects()
        {
            var cache = new MemcachedCache(serverUri);
            var request1 = new TestRequest();
            request1.Name = "Foo";
            cache.Store("mysegment", request1.GetCacheKey(), request1, new TimeSpan(0, 0, 30));

            var request2 = new TestRequest();
            request2.Name = "Bar";
            cache.Store(null, request2.GetCacheKey(), request2, new TimeSpan(0, 0, 30));

            var retrieved = cache.Get<TestRequest>("mysegment", request1.GetCacheKey(), request1);
            Assert.That(() => retrieved.Name,
                Is.EqualTo(request1.Name));

            retrieved = cache.Get<TestRequest>(null, request2.GetCacheKey(), request2);
            Assert.That(() => retrieved.Name,
                Is.EqualTo(request2.Name));
        }

        [Test]
        public void It_should_store_and_not_retrieve_expired_objects()
        {
            var cache = new MemcachedCache(serverUri);
            var request = new TestRequest();
            request.Name = "Foo";
            cache.Store(null, request.GetCacheKey(), request, new TimeSpan(0, 0, 0, 1));

            Assert.That(() => cache.Get<TestRequest>(null, request.GetCacheKey(), request),
                Has.Property("Name").EqualTo("Foo"));

            Thread.Sleep(1500);

            Assert.That(() => cache.Get<TestRequest>(null, request.GetCacheKey(), request),
                Is.Null);

        }

        [Test]
        public void It_should_invalidate_all_object_of_type()
        {
            var cache = new MemcachedCache(serverUri);
            var request1 = new TestRequest();
            request1.Name = "Foo";
            cache.Store(null, request1.GetCacheKey(), request1, new TimeSpan(1, 0, 0));

            var request2 = new TestRequest2();
            request2.Name = "Bar";
            cache.Store(null, request2.GetCacheKey(), request2, new TimeSpan(1, 0, 0));

            Assert.That(() => cache.Get<TestRequest>(null, request1.GetCacheKey(), request1),
                Has.Property("Name").EqualTo("Foo"));

            Assert.That(() => cache.Get<TestRequest2>(null, request2.GetCacheKey(), request2),
                Has.Property("Name").EqualTo("Bar"));

            cache.InvalidateAllObjects(null, typeof(TestRequest2));

            Assert.That(() => cache.Get<TestRequest>(null, request1.GetCacheKey(), request1),
                Is.Not.Null);

            Assert.That(() => cache.Get<TestRequest2>(null, request2.GetCacheKey(), request2),
                Is.Null);
        }

        [TearDown]
        public void TearDown()
        {
            if (memcachedServerProcess != null)
                memcachedServerProcess.Kill();
        }

        public class TestRequest : Request<TestResponse>
        {
            public string Name { get; set; }

            public override string GetCacheKey()
            {
                return GetType().Name + "Name=" + Name;
            }
        }

        public class TestRequest2 : Request<TestResponse>
        {
            public string Name { get; set; }

            public override string GetCacheKey()
            {
                return GetType().Name + "Name=" + Name;
            }
        }
    }
}
