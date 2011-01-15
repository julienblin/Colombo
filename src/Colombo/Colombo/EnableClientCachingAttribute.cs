using System;

namespace Colombo
{
    /// <summary>
    /// Allow a Colombo client to puts responses associated with requests of this type to puts them in a cache.
    /// To enable caching, you have to either register a <see cref="Colombo.Caching.IColomboCache"/> or use 
    /// <seealso cref="Colombo.Facilities.ColomboFacility.EnableInMemoryCaching"/> / <seealso cref="Colombo.Facilities.ColomboFacility.EnableMemcachedCaching"/>.
    /// To allow a request to be put in cache you have to implement <see cref="ICacheable.GetCacheKey"/>.
    /// Cache segments can be control via <see cref="CacheSegmentAttribute"/>, and expiration is time-based or control by <see cref="InvalidateCachedInstancesOfAttribute"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=false)]
    public class EnableClientCachingAttribute : Attribute
    {
        /// <summary>
        /// Number of hours before the cached response will expired.
        /// </summary>
        public int Hours { get; set; }

        /// <summary>
        /// Number of minutes before the cached response will expired.
        /// </summary>
        public int Minutes { get; set; }

        /// <summary>
        /// Number of seconds before the cached response will expired.
        /// </summary>
        public int Seconds { get; set; }

        /// <summary>
        /// Duration before the cached response will expired.
        /// </summary>
        public TimeSpan Duration
        {
            get
            {
                if (Hours == 0 && Minutes == 0 && Seconds == 0)
                {
                    throw new ColomboException("You need to specify at least an hour value, a minute value or a second value.");
                }

                return new TimeSpan(0, Hours, Minutes, Seconds);
            }
        }
    }
}
