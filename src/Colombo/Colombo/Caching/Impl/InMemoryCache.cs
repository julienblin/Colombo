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

        private readonly string segment;
        private readonly Timer scavengingTimer;

        public InMemoryCache(string segment)
        {
            this.segment = segment;
            this.scavengingTimer = new Timer(ScavengingTimeInMilliseconds);
            scavengingTimer.AutoReset = true;
            scavengingTimer.Elapsed += ScavengingTimerElapsed;
            scavengingTimer.Start();
        }

        private Dictionary<string, CacheData> data = new Dictionary<string, CacheData>();

        public void Store(string cacheKey, object @object, TimeSpan duration)
        {
            if (string.IsNullOrEmpty(cacheKey)) throw new ArgumentNullException("cacheKey");
            if (@object == null) throw new ArgumentNullException("object");
            if (duration.Equals(TimeSpan.MaxValue)) throw new ArgumentException("duration cannot be TimeSpan.MaxValue");
            Contract.EndContractBlock();

            lock (syncRoot)
            {
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

        public T Get<T>(string cacheKey) where T : class
        {
            if (string.IsNullOrEmpty(cacheKey)) throw new ArgumentNullException("cacheKey");
            Contract.EndContractBlock();

            lock (syncRoot)
            {
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

        public string Segment
        {
            get { return segment; }
        }

        public int Count { get { return data.Count; } }

        public override string ToString()
        {
            return string.Format("{0} for segment '{1}' - {2} objects.", GetType(), Segment, Count);
        }

        public void ScavengingTimerElapsed(object sender, ElapsedEventArgs e)
        {
            lock (syncRoot)
            {
                var allExpiredItems = data.Where(x => x.Value.Expiration < DateTime.UtcNow).AsParallel().ToArray();
                Parallel.ForEach(allExpiredItems, (expiredItem) => data.Remove(expiredItem.Key));
            }
        }

        private class CacheData
        {
            public Object @Object { get; set; }
            public DateTime Expiration { get; set; }
        }
    }
}
