using System;
using System.Diagnostics.Contracts;

namespace Colombo.Caching
{
    /// <summary>
    /// Represents a cache that can be used to store responses associated with requests.
    /// </summary>
    [ContractClass(typeof(Contracts.ColomboCacheContract))]
    public interface IColomboCache
    {
        /// <summary>
        /// Store an object inside the cache.
        /// </summary>
        /// <param name="segment">The segment to use. Can be null for default segment.</param>
        /// <param name="cacheKey">The key for which the object will be store. Must be unique per Cache segment.</param>
        /// <param name="object">The object to store.</param>
        /// <param name="duration">The duration for which the object will be valid.</param>
        void Store(string segment, string cacheKey, object @object, TimeSpan duration);

        /// <summary>
        /// Get an object from the cache.
        /// </summary>
        /// <param name="segment">The segment to use. Can be null for default segment.</param>
        /// <param name="objectType">The type of the object to retrieve.</param>
        /// <param name="cacheKey">The key associated with the object.</param>
        /// <returns>The object if it's in the cache and no expired, null otherwise.</returns>
        object Get(string segment, Type objectType, string cacheKey);

        /// <summary>
        /// Flush all objects of a specific type in a segment.
        /// </summary>
        /// <param name="segment">The segment to use. Can be null for default segment.</param>
        /// <param name="objectType">The type of the objects to flush.</param>
        void Flush(string segment, Type objectType);

        /// <summary>
        /// Flush the entire cache.
        /// </summary>
        void FlushAll();
    }
}
