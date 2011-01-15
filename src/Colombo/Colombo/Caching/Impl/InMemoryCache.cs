﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace Colombo.Caching.Impl
{
    public class InMemoryCache : IColomboCache
    {
        public const double ScavengingTimeInMilliseconds = 10 * 60 * 1000;

        private readonly object syncRoot = new object();

        private readonly Timer scavengingTimer;

        public InMemoryCache()
        {
            scavengingTimer = new Timer(ScavengingTimeInMilliseconds) {AutoReset = true};
            scavengingTimer.Elapsed += ScavengingTimerElapsed;
            scavengingTimer.Start();
        }

        private Dictionary<string, Dictionary<string, CacheData>> segments = new Dictionary<string, Dictionary<string, CacheData>>();

        public void Store(string segment, string cacheKey, object @object, TimeSpan duration)
        {
            if (string.IsNullOrEmpty(cacheKey)) throw new ArgumentNullException("cacheKey");
            if (@object == null) throw new ArgumentNullException("object");
            if (duration.Equals(TimeSpan.MaxValue)) throw new ArgumentException("duration cannot be TimeSpan.MaxValue");
            Contract.EndContractBlock();

            lock (syncRoot)
            {
                var data = GetSegmentData(segment);
                var expiration = DateTime.UtcNow.Add(duration);
                var finalCacheKey = string.Concat(@object.GetType().FullName, cacheKey);
                if (data.ContainsKey(finalCacheKey))
                {
                    data[finalCacheKey].Object = @object;
                    data[finalCacheKey].Expiration = expiration;
                }
                else
                {
                    data[finalCacheKey] = new CacheData { Object = @object, Expiration = expiration };
                }
            }
        }

        public object Get(string segment, Type objectType, string cacheKey)
        {
            if (objectType == null) throw new ArgumentNullException("objectType");
            if (string.IsNullOrEmpty(cacheKey)) throw new ArgumentNullException("cacheKey");
            Contract.EndContractBlock();

            lock (syncRoot)
            {
                var data = GetSegmentData(segment);
                var finalCacheKey = string.Concat(objectType.FullName, cacheKey);
                if (data.ContainsKey(finalCacheKey))
                {
                    var cacheData = data[finalCacheKey];
                    if (cacheData.Expiration < DateTime.UtcNow)
                    {
                        data.Remove(finalCacheKey);
                        return null;
                    }

                    return cacheData.Object;
                }

                return null;
            }
        }

        public void Flush(string segment, Type objectType)
        {
            if (objectType == null) throw new ArgumentNullException("objectType");
            Contract.EndContractBlock();

            lock (syncRoot)
            {
                var data = GetSegmentData(segment);
                var allItemsOfType = data.Where(x => x.Value.Object.GetType().Equals(objectType)).AsParallel().ToArray();
                Parallel.ForEach(allItemsOfType, item => data.Remove(item.Key));
            }
        }

        public void FlushAll()
        {
            lock (syncRoot)
            {
                segments = new Dictionary<string, Dictionary<string, CacheData>>();
            }
        }

        private Dictionary<string, CacheData> GetSegmentData(string segment)
        {
            if (segments.ContainsKey(segment ?? string.Empty))
                return segments[segment ?? string.Empty];

            var segmentData = new Dictionary<string, CacheData>();
            segments[segment ?? string.Empty] = segmentData;
            return segmentData;
        }


        public int Count { get { return segments.Sum(x => x.Value.Count); } }

        public void ScavengingTimerElapsed(object sender, ElapsedEventArgs e)
        {
            lock (syncRoot)
            {
                foreach (var segment in segments.Keys)
                {
                    var data = GetSegmentData(segment);
                    var allExpiredItems = data.Where(x => x.Value.Expiration < DateTime.UtcNow).AsParallel().ToArray();
                    Parallel.ForEach(allExpiredItems, expiredItem => data.Remove(expiredItem.Key));
                }
            }
        }

        private class CacheData
        {
            public Object @Object { get; set; }
            public DateTime Expiration { get; set; }
        }
    }
}
