using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Colombo.Caching.Impl;
using Castle.Windsor;
using Castle.MicroKernel.Registration;
using Colombo.Caching;

namespace Colombo.Tests.Caching.Impl
{
    [TestFixture]
    public class KernelCacheFactoryTest
    {
        [Test]
        public void It_should_ensure_that_it_has_a_IKernel()
        {
            Assert.That(() => new KernelCacheFactory(null),
                Throws.Exception.TypeOf<ArgumentNullException>()
                .With.Message.Contains("kernel"));
        }

        [Test]
        public void It_should_resolve_ICache_from_Kernel()
        {
            var container = new WindsorContainer();
            container.Register(
                Component.For<ICache>().ImplementedBy<TestCache>().LifeStyle.Transient
            );

            var factory = new KernelCacheFactory(container.Kernel);

            var fooCache = factory.GetCacheForSegment("foo");

            Assert.That(() => fooCache,
                Is.Not.Null);
            Assert.That(() => fooCache.Segment,
                Is.EqualTo("foo"));

            var fooCache2 = factory.GetCacheForSegment("foo");
            Assert.That(() => fooCache2,
                Is.SameAs(fooCache));

            var barCache = factory.GetCacheForSegment("bar");
            Assert.That(() => barCache,
                Is.Not.Null);
            Assert.That(() => barCache.Segment,
                Is.EqualTo("bar"));
            Assert.That(() => barCache,
                Is.Not.SameAs(fooCache));
        }

        [Test]
        public void It_should_throw_an_exception_if_unable_to_resolve_ICache()
        {
            var container = new WindsorContainer();
            var factory = new KernelCacheFactory(container.Kernel);

            Assert.That(() => factory.GetCacheForSegment("foo"),
                Throws.Exception.TypeOf<ColomboException>()
                .With.Message.Contains("ICache"));
        }

        public class TestCache : ICache
        {
            public void Store(string cacheKey, object @object, TimeSpan duration)
            {
                throw new NotImplementedException();
            }

            public T Get<T>(string cacheKey) where T : class
            {
                throw new NotImplementedException();
            }

            public void InvalidateAllObjects<T>()
            {
                throw new NotImplementedException();
            }

            public void InvalidateAllObjects(Type t)
            {
                throw new NotImplementedException();
            }

            public string Segment { get; set; }
        }

    }
}
