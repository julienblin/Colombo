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
using System.Threading;
using Colombo.Caching.Impl;
using NUnit.Framework;

namespace Colombo.Tests.Caching.Impl
{
    [TestFixture]
    public class InMemoryCacheTest : BaseTest
    {

        [Test]
        public void It_should_throw_an_exception_when_cacheKey_or_object_is_null_or_duration_is_max()
        {
            var cache = new InMemoryCache();

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
            var cache = new InMemoryCache();

            var @object1 = new object();

            cache.Store(null, "foo", @object1, new TimeSpan(1, 0, 0));

            Assert.That(cache.Get(null, typeof(object), "foo"), Is.SameAs(@object1));
            Assert.That(cache.Get(null, typeof(object), "foo"), Is.SameAs(@object1));
            Assert.That(cache.Get(null, typeof(object), "bar"), Is.Null);

            var @object2 = new object();
            cache.Store("another", "foo", @object2, new TimeSpan(1, 0, 0));
            Assert.That(cache.Get("another", typeof(object), "foo"), Is.Not.SameAs(@object1));
        }

        [Test]
        public void It_should_store_and_not_retrieve_expired_objects()
        {
            var cache = new InMemoryCache();

            var @object = new object();

            cache.Store(null, "foo", @object, new TimeSpan(0, 0, 0, 0, 500));

            Assert.That(cache.Get(null, typeof(object), "foo"), Is.SameAs(@object));

            Thread.Sleep(1000);

            Assert.That(cache.Get(null, typeof(object), "foo"), Is.Null);
        }

        [Test]
        public void It_should_scavenge_appropriately()
        {
            var cache = new InMemoryCache();

            var @object = new object();

            cache.Store(null, "foo", @object, new TimeSpan(0, 0, 0, 0, 300));
            cache.Store("asegment", "bar", @object, new TimeSpan(0, 0, 0, 1));
            Assert.AreEqual(2, cache.Count);
            Thread.Sleep(500);
            cache.ScavengingTimerElapsed(null, null);

            Assert.AreEqual(1, cache.Count);
            Assert.That(cache.Get("asegment", typeof(object), "bar"), Is.Not.Null);
        }

        [Test]
        public void It_should_flush_all_object_of_type()
        {
            var cache = new InMemoryCache();
            var testObject1 = new TestObject1();
            var testObject2 = new TestObject2();
            var testObject3 = new TestObject3();

            cache.Store(null, "1", testObject1, new TimeSpan(1, 0, 0));
            cache.Store(null, "2", testObject2, new TimeSpan(1, 0, 0));
            cache.Store(null, "3", testObject3, new TimeSpan(1, 0, 0));

            cache.Flush(null, typeof(TestObject1));
            Assert.IsNull(cache.Get(null, typeof(TestObject1), "1"));
            Assert.IsNotNull(cache.Get(null, typeof(TestObject2), "2"));
            Assert.IsNotNull(cache.Get(null, typeof(TestObject3), "3"));

            cache.Flush(null, typeof(TestObject2));
            Assert.IsNull(cache.Get(null, typeof(TestObject2), "2"));
            Assert.IsNotNull(cache.Get(null, typeof(TestObject3), "3"));
        }

        [Test]
        public void It_should_flush_all()
        {
            var cache = new InMemoryCache();
            var testObject1 = new TestObject1();
            var testObject2 = new TestObject2();
            var testObject3 = new TestObject3();

            cache.Store(null, "1", testObject1, new TimeSpan(1, 0, 0));
            cache.Store(null, "2", testObject2, new TimeSpan(1, 0, 0));
            cache.Store(null, "3", testObject3, new TimeSpan(1, 0, 0));

            cache.FlushAll();
            Assert.IsNull(cache.Get(null, typeof(TestObject1), "1"));
            Assert.IsNull(cache.Get(null, typeof(TestObject2), "2"));
            Assert.IsNull(cache.Get(null, typeof(TestObject3), "3"));
        }

        public class TestObject1 { }
        public class TestObject2 { }
        public class TestObject3 : TestObject2 { }
    }
}
