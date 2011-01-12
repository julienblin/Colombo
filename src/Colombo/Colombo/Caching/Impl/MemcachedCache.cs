using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using BeIT.MemCached;
using System.IO;
using System.Xml;
using System.Runtime.Serialization;
using System.Diagnostics.Contracts;

namespace Colombo.Caching.Impl
{
    public class MemcachedCache : IColomboCache
    {
        private readonly MemcachedClient memcachedClient;

        public MemcachedCache(string serverUri)
            : this(new string[] {serverUri})
        {
        }

        public MemcachedCache(string[] servers)
        {
            memcachedClient = new MemcachedClient(@"Colombo", servers);
        }

        private string GetKeyForCurrentIncrement(string segment, string type)
        {
            return string.Format("{0}.{1}.current", segment, type);
        }

        private string GetFinalCacheKey(string segment, string type, string cacheKey)
        {
            var keyForCurrentIncrement = GetKeyForCurrentIncrement(segment, type);
            ulong currentIncrementValue = 0;

            var currentIncrementValueFromMemcache = memcachedClient.Get(keyForCurrentIncrement);
            if (currentIncrementValueFromMemcache != null)
            {
                currentIncrementValue = (ulong)currentIncrementValueFromMemcache;
            }
            else
            {
                memcachedClient.Add(keyForCurrentIncrement, (ulong)0);
            }
            return string.Format("{0}.{1}.{2}.{3}", segment, type, currentIncrementValue, cacheKey);
        }

        public void Store(string segment, string cacheKey, object @object, TimeSpan duration)
        {
            if (string.IsNullOrEmpty(cacheKey)) throw new ArgumentNullException("cacheKey");
            if (@object == null) throw new ArgumentNullException("object");
            if (duration.Equals(TimeSpan.MaxValue)) throw new ArgumentException("duration cannot be TimeSpan.MaxValue");
            Contract.EndContractBlock();

            var finalCachekey = GetFinalCacheKey(segment, @object.GetType().FullName, cacheKey);

            using(var backing = new StringWriter())
            using (var writer = new XmlTextWriter(backing))
            {
                var serializer = new DataContractSerializer(@object.GetType());
                serializer.WriteObject(writer, @object);
                memcachedClient.Set(finalCachekey, backing.ToString(), duration);
            }
        }

        public object Get(string segment, Type objectType, string cacheKey)
        {
            var finalCachekey = GetFinalCacheKey(segment, objectType.FullName, cacheKey);
            var objectStr = memcachedClient.Get(finalCachekey) as string;
            if (objectStr == null)
                return null;

            using(var backing = new StringReader(objectStr))
            using (var reader = new XmlTextReader(backing))
            {
                var serializer = new DataContractSerializer(objectType);
                return serializer.ReadObject(reader);
            }
        }

        public void InvalidateAllObjects(string segment, Type cacheType)
        {
            var keyForCurrentIncrement = GetKeyForCurrentIncrement(segment, cacheType.FullName);
            memcachedClient.Increment(keyForCurrentIncrement, 1);
        }
    }
}
