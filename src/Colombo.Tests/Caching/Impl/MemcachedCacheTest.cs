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
using System.Diagnostics;
using System.IO;
using System.Threading;
using Colombo.Alerts;
using Colombo.Caching.Impl;
using NUnit.Framework;
using Rhino.Mocks;

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
            var processStartInfo = new ProcessStartInfo(memcachedServerPath) { UseShellExecute = false, CreateNoWindow = true };
            memcachedServerProcess = Process.Start(processStartInfo);
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
                var cache = new MemcachedCache(serverUri) { Alerters = new IColomboAlerter[] { alerter } };
                var request1 = new TestRequest { Name = "Foo" };
                cache.Store("mysegment", request1.GetCacheKey(), request1, new TimeSpan(0, 0, 30));

                var request2 = new TestRequest { Name = "Bar" };
                cache.Store(null, request2.GetCacheKey(), request2, new TimeSpan(0, 0, 30));

                var retrieved = (TestRequest)cache.Get("mysegment", typeof(TestRequest), request1.GetCacheKey());
                Assert.That(retrieved.Name, Is.EqualTo(request1.Name));

                retrieved = (TestRequest)cache.Get(null, typeof(TestRequest), request2.GetCacheKey());
                Assert.That(retrieved.Name, Is.EqualTo(request2.Name));
            });
        }

        [Test]
        public void It_should_store_and_not_retrieve_expired_objects()
        {
            var cache = new MemcachedCache(serverUri);
            var request = new TestRequest { Name = "Foo" };
            cache.Store(null, request.GetCacheKey(), request, new TimeSpan(0, 0, 0, 1));

            Assert.That(cache.Get(null, typeof(TestRequest), request.GetCacheKey()),
                Has.Property("Name").EqualTo("Foo"));

            Thread.Sleep(1500);

            Assert.That(cache.Get(null, typeof(TestRequest), request.GetCacheKey()),
                Is.Null);

        }

        [Test]
        public void It_should_flush_all_object_of_type()
        {
            var cache = new MemcachedCache(serverUri);
            var request1 = new TestRequest { Name = "Foo" };
            cache.Store(null, request1.GetCacheKey(), request1, new TimeSpan(1, 0, 0));

            var request2 = new TestRequest2 { Name = "Bar" };
            cache.Store(null, request2.GetCacheKey(), request2, new TimeSpan(1, 0, 0));

            Assert.That(cache.Get(null, typeof(TestRequest), request1.GetCacheKey()),
                Has.Property("Name").EqualTo("Foo"));

            Assert.That(cache.Get(null, typeof(TestRequest2), request2.GetCacheKey()),
                Has.Property("Name").EqualTo("Bar"));

            cache.Flush(null, typeof(TestRequest2));

            Assert.That(cache.Get(null, typeof(TestRequest), request1.GetCacheKey()), Is.Not.Null);
            Assert.That(cache.Get(null, typeof(TestRequest2), request2.GetCacheKey()), Is.Null);
        }

        [Test]
        public void It_should_flush_all()
        {
            var cache = new MemcachedCache(serverUri);
            var request1 = new TestRequest { Name = "Foo" };
            cache.Store(null, request1.GetCacheKey(), request1, new TimeSpan(1, 0, 0));

            var request2 = new TestRequest2 { Name = "Bar" };
            cache.Store(null, request2.GetCacheKey(), request2, new TimeSpan(1, 0, 0));

            cache.FlushAll();

            Assert.That(cache.Get(null, typeof(TestRequest), request1.GetCacheKey()), Is.Null);
            Assert.That(cache.Get(null, typeof(TestRequest2), request2.GetCacheKey()), Is.Null);
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
                var cache = new MemcachedCache(serverUri) { Alerters = new[] { alerter } };

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
