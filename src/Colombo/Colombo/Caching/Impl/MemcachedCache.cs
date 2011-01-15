using System;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using BeIT.MemCached;
using Castle.Core.Logging;
using Colombo.Alerts;

namespace Colombo.Caching.Impl
{
    /// <summary>
    /// Implementation of a <see cref="IColomboCache"/> that stores values in a memcached server.
    /// </summary>
    /// <seealso cref="Colombo.Facilities.ColomboFacility.EnableMemcachedCaching"/>
    /// <remarks>
    /// Memcached: http://memcached.org
    /// 
    /// This implementation encode the keys in a non human-readable format.
    /// This cache is fail-safe: no exceptions should be thrown even if the memcached servers are unreachable.
    /// Only <see cref="Colombo.Alerts.MemcachedUnreachableAlert"/> will be emitted.
    /// </remarks>
    public class MemcachedCache : IColomboCache
    {
        private ILogger logger = NullLogger.Instance;
        /// <summary>
        /// Logger instance.
        /// </summary>
        public ILogger Logger
        {
            get { return logger; }
            set { logger = value; }
        }

        private IColomboAlerter[] alerters = new IColomboAlerter[0];
        /// <summary>
        /// List of alerters to use when Memcached servers are unreachable.
        /// Will emit <see cref="Colombo.Alerts.MemcachedUnreachableAlert"/>.
        /// </summary>
        public IColomboAlerter[] Alerters
        {
            get { return alerters; }
            set
            {
                if (value == null) throw new ArgumentNullException("Alerters");
                Contract.EndContractBlock();

                alerters = value;

                if (!Logger.IsInfoEnabled) return;

                if (alerters.Length == 0)
                    Logger.Info("No alerters has been registered for the MemcachedCache.");
                else
                    Logger.InfoFormat("MemcachedCache monitoring with the following alerters: {0}", string.Join(", ", alerters.Select(x => x.GetType().Name)));
            }
        }

        private readonly string[] servers;
        private readonly MemcachedClient memcachedClient;
        private readonly SHA1 sha1 = new SHA1CryptoServiceProvider();

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="serverUri">Uri of the memcached server to use.</param>
        public MemcachedCache(string serverUri)
            : this(new[] { serverUri })
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="servers">List of memcached servers to use.</param>
        public MemcachedCache(string[] servers)
        {
            this.servers = servers;
            memcachedClient = new MemcachedClient(@"Colombo", servers);
        }

        private string GetEncodedKey(string key)
        {
            return Convert.ToBase64String(sha1.ComputeHash(Encoding.UTF8.GetBytes(key)));
        }

        private string GetKeyForCurrentIncrement(string segment, string type)
        {
            return GetEncodedKey(string.Format("{0}.{1}.current", segment, type));
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
            return GetEncodedKey(string.Format("{0}.{1}.{2}.{3}", segment, type, currentIncrementValue, cacheKey));
        }

        /// <summary>
        /// Store an object inside the cache.
        /// </summary>
        /// <param name="segment">The segment to use. Can be null for default segment.</param>
        /// <param name="cacheKey">The key for which the object will be store. Must be unique per Cache segment.</param>
        /// <param name="object">The object to store.</param>
        /// <param name="duration">The duration for which the object will be valid.</param>
        public void Store(string segment, string cacheKey, object @object, TimeSpan duration)
        {
            if (string.IsNullOrEmpty(cacheKey)) throw new ArgumentNullException("cacheKey");
            if (@object == null) throw new ArgumentNullException("object");
            if (duration.Equals(TimeSpan.MaxValue)) throw new ArgumentException("duration cannot be TimeSpan.MaxValue");
            Contract.EndContractBlock();

            var finalCachekey = GetFinalCacheKey(segment, @object.GetType().FullName, cacheKey);

            using (var backing = new StringWriter())
            using (var writer = new XmlTextWriter(backing))
            {
                var serializer = new DataContractSerializer(@object.GetType());
                serializer.WriteObject(writer, @object);
                if (!memcachedClient.Set(finalCachekey, backing.ToString(), duration))
                {
                    var alert = new MemcachedUnreachableAlert(Environment.MachineName, servers);
                    Logger.Warn(alert.ToString());
                    foreach (var alerter in Alerters)
                    {
                        alerter.Alert(alert);
                    }
                }
            }
        }

        /// <summary>
        /// Get an object from the cache.
        /// </summary>
        /// <param name="segment">The segment to use. Can be null for default segment.</param>
        /// <param name="objectType">The type of the object to retrieve.</param>
        /// <param name="cacheKey">The key associated with the object.</param>
        /// <returns>The object if it's in the cache and no expired, null otherwise.</returns>
        public object Get(string segment, Type objectType, string cacheKey)
        {
            var finalCachekey = GetFinalCacheKey(segment, objectType.FullName, cacheKey);
            var objectStr = memcachedClient.Get(finalCachekey) as string;
            if (objectStr == null)
                return null;

            using (var backing = new StringReader(objectStr))
            using (var reader = new XmlTextReader(backing))
            {
                var serializer = new DataContractSerializer(objectType);
                return serializer.ReadObject(reader);
            }
        }

        /// <summary>
        /// Flush all objects of a specific type in a segment.
        /// </summary>
        /// <param name="segment">The segment to use. Can be null for default segment.</param>
        /// <param name="objectType">The type of the objects to flush.</param>
        public void Flush(string segment, Type objectType)
        {
            if (objectType == null) throw new ArgumentNullException("objectType");
            Contract.EndContractBlock();

            var keyForCurrentIncrement = GetKeyForCurrentIncrement(segment, objectType.FullName);
            memcachedClient.Increment(keyForCurrentIncrement, 1);
        }

        /// <summary>
        /// Flush the entire cache.
        /// </summary>
        public void FlushAll()
        {
            memcachedClient.FlushAll();
        }
    }
}
