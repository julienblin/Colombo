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
        public void It_should_throw_an_exception_when_cacheKey_or_object_is_null_or_duration_is_max()
        {
            var cache = new InMemoryCache();

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
            var cache = new InMemoryCache();

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
            var cache = new InMemoryCache();

            var @object = new object();

            cache.Store("foo", @object, new TimeSpan(0, 0, 0, 0, 500));

            Assert.That(() => cache.Get<object>("foo"),
                Is.SameAs(@object));

            Thread.Sleep(1000);

            Assert.That(() => cache.Get<object>("foo"),
                Is.Null);
        }

        [Test]
        public void It_should_scavenge_appropriately()
        {
            var cache = new InMemoryCache();

            var @object = new object();

            cache.Store("foo", @object, new TimeSpan(0, 0, 0, 0, 300));
            cache.Store("bar", @object, new TimeSpan(0, 0, 0, 1));
            Assert.AreEqual(2, cache.Count);
            Thread.Sleep(500);
            cache.ScavengingTimerElapsed(null, null);

            Assert.AreEqual(1, cache.Count);
            Assert.That(() => cache.Get<object>("bar"),
                Is.Not.Null);
        }

        [Test]
        public void It_should_invalidate_all_object_of_type()
        {
            var cache = new InMemoryCache();
            var testObject1 = new TestObject1();
            var testObject2 = new TestObject2();
            var testObject3 = new TestObject3();

            cache.Store("1", testObject1, new TimeSpan(1, 0, 0));
            cache.Store("2", testObject2, new TimeSpan(1, 0, 0));
            cache.Store("3", testObject3, new TimeSpan(1, 0, 0));

            cache.InvalidateAllObjects<TestObject1>();
            Assert.IsNull(cache.Get<TestObject1>("1"));
            Assert.IsNotNull(cache.Get<TestObject2>("2"));
            Assert.IsNotNull(cache.Get<TestObject3>("3"));

            cache.InvalidateAllObjects(typeof(TestObject2));
            Assert.IsNull(cache.Get<TestObject2>("2"));
            Assert.IsNotNull(cache.Get<TestObject3>("3"));
        }

        public class TestObject1 { }
        public class TestObject2 { }
        public class TestObject3 : TestObject2 { }
    }
}
