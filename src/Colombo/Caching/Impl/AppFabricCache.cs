using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using Castle.Core.Logging;
using Colombo.Alerts;
using Microsoft.ApplicationServer.Caching;

namespace Colombo.Caching.Impl
{
    public class AppFabricCache : IColomboCache
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
                    Logger.Info("No alerters has been registered for the AppFabricCache.");
                else
                    Logger.InfoFormat("AppFabricCache monitoring with the following alerters: {0}", string.Join(", ", alerters.Select(x => x.GetType().Name)));
            }
        }

        private readonly string cacheName;
        private readonly DataCacheFactory cacheFactory;

        public AppFabricCache(string cacheName)
        {
            this.cacheName = cacheName;
            cacheFactory = new DataCacheFactory();
        }

        private DataCache dataCache;

        private DataCache DataCache
        {
            get
            {
                return dataCache ??
                       (dataCache =
                        string.IsNullOrEmpty(cacheName)
                            ? cacheFactory.GetDefaultCache()
                            : cacheFactory.GetCache(cacheName));
            }
        }

        private static string GetKeyForCurrentIncrement(string segment, string type)
        {
            return string.Format("{0}.{1}.current", segment, type);
        }

        private string GetFinalCacheKey(string segment, string type, string cacheKey)
        {
            var keyForCurrentIncrement = GetKeyForCurrentIncrement(segment, type);
            ulong currentIncrementValue = 0;

            var currentIncrementValueFromMemcache = dataCache.Get(keyForCurrentIncrement);
            if (currentIncrementValueFromMemcache != null)
            {
                currentIncrementValue = (ulong)currentIncrementValueFromMemcache;
            }
            else
            {
                dataCache.Add(keyForCurrentIncrement, (ulong)0);
            }
            return string.Format("{0}.{1}.{2}.{3}", segment, type, currentIncrementValue, cacheKey);
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

            try
            {
                var finalCachekey = GetFinalCacheKey(segment, @object.GetType().FullName, cacheKey);
                DataCache.Put(finalCachekey, @object, duration);
            }
            catch (Exception ex)
            {
                Logger.Warn("Error when storing object to AppFabric Cache", ex);
                var alert = new AppFabricCacheErrorAlert(Environment.MachineName, ex);
                foreach (var alerter in Alerters)
                {
                    alerter.Alert(alert);
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
            try
            {
                var finalCachekey = GetFinalCacheKey(segment, objectType.FullName, cacheKey);
                return DataCache.Get(finalCachekey);
            }
            catch (Exception ex)
            {
                Logger.Warn("Error when retrieving object from AppFabric Cache", ex);
                var alert = new AppFabricCacheErrorAlert(Environment.MachineName, ex);
                foreach (var alerter in Alerters)
                {
                    alerter.Alert(alert);
                }
                return null;
            }
        }

        /// <summary>
        /// Flush all objects of a specific type in a segment.
        /// </summary>
        /// <param name="segment">The segment to use. Can be null for default segment.</param>
        /// <param name="objectType">The type of the objects to flush.</param>
        public void Flush(string segment, Type objectType)
        {
            try
            {
                var keyForCurrentIncrement = GetKeyForCurrentIncrement(segment, objectType.FullName);
                DataCacheLockHandle lockHandle;
                var currentIncrementValueFromMemcache = dataCache.GetAndLock(keyForCurrentIncrement, TimeSpan.FromSeconds(5), out lockHandle);
                if (currentIncrementValueFromMemcache != null)
                {
                    DataCache.PutAndUnlock(keyForCurrentIncrement, ((int)currentIncrementValueFromMemcache) + 1, lockHandle);
                }
            }
            catch (Exception ex)
            {
                Logger.Warn("Error when flushing object in AppFabric Cache", ex);
                var alert = new AppFabricCacheErrorAlert(Environment.MachineName, ex);
                foreach (var alerter in Alerters)
                {
                    alerter.Alert(alert);
                }
            }
        }

        /// <summary>
        /// Flush the entire cache.
        /// </summary>
        public void FlushAll()
        {
            throw new NotSupportedException("AppFabric Cache cannot flush the cache from the client.");
        }
    }
}
