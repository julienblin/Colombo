using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.IO;
using System.Diagnostics;
using Colombo.Caching.Impl;
using System.Threading;
using Rhino.Mocks;
using Colombo.Alerts;

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
            var mocks = new MockRepository();

            var alerter = mocks.StrictMock<IColomboAlerter>();

            With.Mocks(mocks).Expecting(() =>
            {

            }).Verify(() =>
            {
                var cache = new MemcachedCache(serverUri);
                cache.Alerters = new IColomboAlerter[] { alerter };
                var request1 = new TestRequest();
                request1.Name = "Foo";
                cache.Store("mysegment", request1.GetCacheKey(), request1, new TimeSpan(0, 0, 30));

                var request2 = new TestRequest();
                request2.Name = "Bar";
                cache.Store(null, request2.GetCacheKey(), request2, new TimeSpan(0, 0, 30));

                var retrieved = (TestRequest)cache.Get("mysegment", typeof(TestRequest), request1.GetCacheKey());
                Assert.That(() => retrieved.Name,
                    Is.EqualTo(request1.Name));

                retrieved = (TestRequest)cache.Get(null, typeof(TestRequest), request2.GetCacheKey());
                Assert.That(() => retrieved.Name,
                    Is.EqualTo(request2.Name));
            });
        }

        [Test]
        public void It_should_store_and_not_retrieve_expired_objects()
        {
            var cache = new MemcachedCache(serverUri);
            var request = new TestRequest();
            request.Name = "Foo";
            cache.Store(null, request.GetCacheKey(), request, new TimeSpan(0, 0, 0, 1));

            Assert.That(() => (TestRequest)cache.Get(null, typeof(TestRequest), request.GetCacheKey()),
                Has.Property("Name").EqualTo("Foo"));

            Thread.Sleep(1500);

            Assert.That(() => (TestRequest)cache.Get(null, typeof(TestRequest), request.GetCacheKey()),
                Is.Null);

        }

        [Test]
        public void It_should_flush_all_object_of_type()
        {
            var cache = new MemcachedCache(serverUri);
            var request1 = new TestRequest();
            request1.Name = "Foo";
            cache.Store(null, request1.GetCacheKey(), request1, new TimeSpan(1, 0, 0));

            var request2 = new TestRequest2();
            request2.Name = "Bar";
            cache.Store(null, request2.GetCacheKey(), request2, new TimeSpan(1, 0, 0));

            Assert.That(() => (TestRequest)cache.Get(null, typeof(TestRequest), request1.GetCacheKey()),
                Has.Property("Name").EqualTo("Foo"));

            Assert.That(() => (TestRequest2)cache.Get(null, typeof(TestRequest2), request2.GetCacheKey()),
                Has.Property("Name").EqualTo("Bar"));

            cache.Flush(null, typeof(TestRequest2));

            Assert.That(() => (TestRequest)cache.Get(null, typeof(TestRequest), request1.GetCacheKey()),
                Is.Not.Null);

            Assert.That(() => (TestRequest2)cache.Get(null, typeof(TestRequest2), request2.GetCacheKey()),
                Is.Null);
        }

        [Test]
        public void It_should_flush_all()
        {
            var cache = new MemcachedCache(serverUri);
            var request1 = new TestRequest();
            request1.Name = "Foo";
            cache.Store(null, request1.GetCacheKey(), request1, new TimeSpan(1, 0, 0));

            var request2 = new TestRequest2();
            request2.Name = "Bar";
            cache.Store(null, request2.GetCacheKey(), request2, new TimeSpan(1, 0, 0));

            cache.FlushAll();

            Assert.That(() => (TestRequest2)cache.Get(null, typeof(TestRequest), request1.GetCacheKey()),
                Is.Null);

            Assert.That(() => (TestRequest2)cache.Get(null, typeof(TestRequest2), request2.GetCacheKey()),
                Is.Null);
        }

        [Test]
        public void It_should_not_throw_exceptions_when_server_is_offline_and_issue_alert()
        {
            var mocks = new MockRepository();

            var alerter = mocks.StrictMock<IColomboAlerter>();

            With.Mocks(mocks).Expecting(() =>
            {
                alerter.Alert(null);
                LastCall.IgnoreArguments();
                LastCall.Constraints(
                    Rhino.Mocks.Constraints.Is.TypeOf<MemcachedUnreachableAlert>()
                );
            }).Verify(() =>
            {
                var cache = new MemcachedCache(serverUri);
                cache.Alerters = new IColomboAlerter[] { alerter };

                memcachedServerProcess.Kill();
                memcachedServerProcess = null;

                var @object = new object();
                cache.Store(null, "0", @object, new TimeSpan(1, 0, 0));
            });
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
