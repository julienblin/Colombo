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
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace Colombo.Caching.Impl
{
    /// <summary>
    /// Implementation of a <see cref="IColomboCache"/> that stores values in-memory.
    /// </summary>
    /// <seealso cref="Colombo.Facilities.ColomboFacility.EnableInMemoryCaching"/>
    public class InMemoryCache : IColomboCache
    {
        /// <summary>
        /// Interval in milliseconds between scavenging operations. The scavenging empty expired objects from the cache.
        /// </summary>
        public const double ScavengingTimeInMilliseconds = 10 * 60 * 1000;

        private readonly object syncRoot = new object();

        private readonly Timer scavengingTimer;

        /// <summary>
        /// Constructor.
        /// </summary>
        public InMemoryCache()
        {
            scavengingTimer = new Timer(ScavengingTimeInMilliseconds) {AutoReset = true};
            scavengingTimer.Elapsed += ScavengingTimerElapsed;
            scavengingTimer.Start();
        }

        private Dictionary<string, Dictionary<string, CacheData>> segments = new Dictionary<string, Dictionary<string, CacheData>>();

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

        /// <summary>
        /// Get an object from the cache.
        /// </summary>
        /// <param name="segment">The segment to use. Can be null for default segment.</param>
        /// <param name="objectType">The type of the object to retrieve.</param>
        /// <param name="cacheKey">The key associated with the object.</param>
        /// <returns>The object if it's in the cache and no expired, null otherwise.</returns>
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

        /// <summary>
        /// Flush all objects of a specific type in a segment.
        /// </summary>
        /// <param name="segment">The segment to use. Can be null for default segment.</param>
        /// <param name="objectType">The type of the objects to flush.</param>
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

        /// <summary>
        /// Flush the entire cache.
        /// </summary>
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

        /// <summary>
        /// Number of items in the cache, in all segments.
        /// </summary>
        public int Count { get { return segments.Sum(x => x.Value.Count); } }

        internal void ScavengingTimerElapsed(object sender, ElapsedEventArgs e)
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
