using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Core.Logging;
using System.Collections.Concurrent;
using System.Timers;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace Colombo.Caching.Impl
{
    public class InMemoryCache : ICache
    {
        public const double ScavengingTimeInMilliseconds = 10 * 60 * 1000;

        private ILogger logger = NullLogger.Instance;
        public ILogger Logger
        {
            get { return logger; }
            set { logger = value; }
        }

        private object syncRoot = new object();

        private readonly Timer scavengingTimer;

        public InMemoryCache()
        {
            this.scavengingTimer = new Timer(ScavengingTimeInMilliseconds);
            scavengingTimer.AutoReset = true;
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
                DateTime expiration = DateTime.UtcNow.Add(duration);
                if (data.ContainsKey(cacheKey))
                {
                    data[cacheKey].Object = @object;
                    data[cacheKey].Expiration = expiration;
                }
                else
                {
                    data[cacheKey] = new CacheData { Object = @object, Expiration = expiration };
                }
            }
        }

        public T Get<T>(string segment, string cacheKey, Type cacheType) where T : class
        {
            if (string.IsNullOrEmpty(cacheKey)) throw new ArgumentNullException("cacheKey");
            Contract.EndContractBlock();

            lock (syncRoot)
            {
                var data = GetSegmentData(segment);
                if (data.ContainsKey(cacheKey))
                {
                    var cacheData = data[cacheKey];
                    if (cacheData.Expiration < DateTime.UtcNow)
                    {
                        data.Remove(cacheKey);
                        return null;
                    }
                    else
                    {
                        return (T)cacheData.Object;
                    }
                }
                else
                {
                    return null;
                }
            }
        }

        public void InvalidateAllObjects(string segment, Type cacheType)
        {
            lock (syncRoot)
            {
                var data = GetSegmentData(segment);
                var allItemsOfType = data.Where(x => x.Value.Object.GetType().Equals(cacheType)).AsParallel().ToArray();
                Parallel.ForEach(allItemsOfType, (item) => data.Remove(item.Key));
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
                    Parallel.ForEach(allExpiredItems, (expiredItem) => data.Remove(expiredItem.Key));
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
