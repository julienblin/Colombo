using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Colombo.Interceptors;
using NUnit.Framework;

namespace Colombo.Tests
{
    [TestFixture]
    public class CacheSegmentAttributeTest
    {
        [Test]
        public void It_should_throw_an_exception_if_nothing_specified()
        {
            var request = new NothingSpecifiedTestRequest();
            var cacheSegmentAttribute = request.GetCustomAttribute<CacheSegmentAttribute>();

            Assert.That(() => cacheSegmentAttribute.GetCacheSegment(request),
                Throws.Exception.TypeOf<ColomboException>()
                .With.Message.Contains("Name")
                .And.Message.Contains("FromContextKey")
                .And.Message.Contains("FromProperty"));
        }

        [Test]
        public void It_should_throw_an_exception_if_FromContextKey_and_FromProperty_are_specified()
        {
            var request = new BothFromContextKeyAndFromPropertySpecifiedTestRequest();
            var cacheSegmentAttribute = request.GetCustomAttribute<CacheSegmentAttribute>();

            Assert.That(() => cacheSegmentAttribute.GetCacheSegment(request),
                Throws.Exception.TypeOf<ColomboException>()
                .With.Message.Contains("both")
                .And.Message.Contains("FromContextKey")
                .And.Message.Contains("FromProperty"));
        }

        [Test]
        public void It_should_return_if_name_specified()
        {
            var request = new NameSpecifiedTestRequest();
            var cacheSegmentAttribute = request.GetCustomAttribute<CacheSegmentAttribute>();

            Assert.That(cacheSegmentAttribute.GetCacheSegment(request), Is.EqualTo("CacheName"));
        }

        [Test]
        public void It_should_return_if_FromContextKey_specified()
        {
            var request = new NameSpecifiedTestRequest();
            request.Context["ContextKey"] = "CacheName";
            var cacheSegmentAttribute = request.GetCustomAttribute<CacheSegmentAttribute>();

            Assert.That(cacheSegmentAttribute.GetCacheSegment(request), Is.EqualTo("CacheName"));
        }

        [Test]
        public void It_should_return_if_FromContextKey_And_name_specified_and_no_ContextKey()
        {
            var request = new FromContextKeyAndNameSpecifiedTestRequest();
            var cacheSegmentAttribute = request.GetCustomAttribute<CacheSegmentAttribute>();

            Assert.That(cacheSegmentAttribute.GetCacheSegment(request), Is.EqualTo("CacheName"));
        }

        [Test]
        public void It_should_throw_an_exception_if_FromContextKey_specified_and_no_ContextKey_and_no_name()
        {
            var request = new FromContextKeySpecifiedTestRequest();
            var cacheSegmentAttribute = request.GetCustomAttribute<CacheSegmentAttribute>();

            Assert.That(() => cacheSegmentAttribute.GetCacheSegment(request),
                Throws.Exception.TypeOf<ColomboException>()
                .With.Message.Contains("ContextKey"));
        }

        [Test]
        public void It_should_return_if_FromProperty_specified()
        {
            var request = new FromPropertySpecifiedTestRequest();
            request.Id = Guid.NewGuid();
            var cacheSegmentAttribute = request.GetCustomAttribute<CacheSegmentAttribute>();

            Assert.That(cacheSegmentAttribute.GetCacheSegment(request), Is.EqualTo(request.Id.ToString()));
        }

        [Test]
        public void It_should_return_if_FromProperty_And_name_specified_and_property_null()
        {
            var request = new FromPropertyAndNameSpecifiedTestRequest();
            var cacheSegmentAttribute = request.GetCustomAttribute<CacheSegmentAttribute>();

            Assert.That(cacheSegmentAttribute.GetCacheSegment(request), Is.EqualTo("CacheName"));
        }

        [Test]
        public void It_should_throw_if_property_doesnt_exist()
        {
            var request = new FromPropertyDoesntExistSpecifiedTestRequest();
            var cacheSegmentAttribute = request.GetCustomAttribute<CacheSegmentAttribute>();

            Assert.That(() => cacheSegmentAttribute.GetCacheSegment(request),
                Throws.Exception.TypeOf<ColomboException>()
                .With.Message.Contains("Name"));
        }

        [Test]
        public void It_should_throw_if_property_only_have_set()
        {
            var request = new FromPropertySetOnlySpecifiedTestRequest();
            var cacheSegmentAttribute = request.GetCustomAttribute<CacheSegmentAttribute>();

            Assert.That(() => cacheSegmentAttribute.GetCacheSegment(request),
                Throws.Exception.TypeOf<ColomboException>()
                .With.Message.Contains("Name"));
        }

        [CacheSegment]
        public class NothingSpecifiedTestRequest : SideEffectFreeRequest<TestResponse> { }

        [CacheSegment(FromContextKey = "ContextKey", FromProperty = "PropertyName")]
        public class BothFromContextKeyAndFromPropertySpecifiedTestRequest : SideEffectFreeRequest<TestResponse> { }

        [CacheSegment("CacheName")]
        public class NameSpecifiedTestRequest : SideEffectFreeRequest<TestResponse> { }

        [CacheSegment(FromContextKey = "ContextKey")]
        public class FromContextKeySpecifiedTestRequest : SideEffectFreeRequest<TestResponse> { }

        [CacheSegment(FromContextKey = "ContextKey", Name = "CacheName")]
        public class FromContextKeyAndNameSpecifiedTestRequest : SideEffectFreeRequest<TestResponse> { }

        [CacheSegment(FromProperty = "Id")]
        public class FromPropertySpecifiedTestRequest : SideEffectFreeRequest<TestResponse>
        {
            public Guid Id { get; set; }
        }

        [CacheSegment(FromProperty = "Name", Name = "CacheName")]
        public class FromPropertyAndNameSpecifiedTestRequest : SideEffectFreeRequest<TestResponse>
        {
            public string Name { get; set; }
        }

        [CacheSegment(FromProperty = "Name")]
        public class FromPropertyDoesntExistSpecifiedTestRequest : SideEffectFreeRequest<TestResponse>
        {
            
        }

        [CacheSegment(FromProperty = "Name")]
        public class FromPropertySetOnlySpecifiedTestRequest : SideEffectFreeRequest<TestResponse>
        {
            public string Name { private get; set; }
        }
    }
}
