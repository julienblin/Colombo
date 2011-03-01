using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Colombo.Caching.Impl;
using NUnit.Framework;

namespace Colombo.Tests.Caching.Impl
{
    [TestFixture]
    public class AppFabricCacheTest
    {
        [Test]
        public void It_should_throw_an_exception_when_cacheKey_or_object_is_null_or_duration_is_max()
        {
            var cache = new AppFabricCache(null);

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
    }
}
