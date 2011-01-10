using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Colombo.Caching.Impl;
using System.Threading;

namespace Colombo.Tests.Caching.Impl
{
    [TestFixture]
    public class InMemoryCacheTest : BaseTest
    {
        [Test]
        public void It_should_return_the_Segment()
        {
            var cache = new InMemoryCache(null);
            Assert.AreEqual(cache.Segment, null);

            cache = new InMemoryCache("The segment");
            Assert.AreEqual(cache.Segment, "The segment");
        }

        [Test]
        public void It_should_throw_an_exception_when_cacheKey_or_object_is_null_or_duration_is_max()
        {
            var cache = new InMemoryCache(null);

            Assert.That(() => cache.Store("", new Object(), TimeSpan.MinValue),
                Throws.Exception.TypeOf<ArgumentNullException>()
                .With.Message.Contains("cacheKey"));

            Assert.That(() => cache.Store("key", null, TimeSpan.MinValue),
                Throws.Exception.TypeOf<ArgumentNullException>()
                .With.Message.Contains("object"));

            Assert.That(() => cache.Store("key", new Object(), TimeSpan.MaxValue),
                Throws.Exception.TypeOf<ArgumentException>()
                .With.Message.Contains("duration"));
        }

        [Test]
        public void It_should_store_and_retrieve_objects()
        {
            var cache = new InMemoryCache(null);

            var @object = new object();

            cache.Store("foo", @object, new TimeSpan(1, 0, 0));

            Assert.That(() => cache.Get<object>("foo"),
                Is.SameAs(@object));

            Assert.That(() => cache.Get<object>("foo"),
                Is.SameAs(@object));

            Assert.That(() => cache.Get<object>("bar"),
                Is.Null);
        }

        [Test]
        public void It_should_store_and_not_retrieve_expired_objects()
        {
            var cache = new InMemoryCache(null);

            var @object = new object();

            cache.Store("foo", @object, new TimeSpan(0, 0, 0, 0, 500));

            Assert.That(() => cache.Get<object>("foo"),
                Is.SameAs(@object));

            Thread.Sleep(500);

            Assert.That(() => cache.Get<object>("foo"),
                Is.Null);
        }

        [Test]
        public void It_should_scavenge_appropriately()
        {
            var cache = new InMemoryCache(null);

            var @object = new object();

            cache.Store("foo", @object, new TimeSpan(0, 0, 0, 0, 300));
            cache.Store("bar", @object, new TimeSpan(0, 0, 0, 1));
            Assert.AreEqual(cache.Count, 2);
            Thread.Sleep(300);
            cache.ScavengingTimerElapsed(null, null);
            
            Assert.AreEqual(cache.Count, 1);
            Assert.That(() => cache.Get<object>("bar"),
                Is.Not.Null);
        }
    }
}
