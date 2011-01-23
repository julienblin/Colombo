namespace Colombo
{
    /// <summary>
    /// Represents an item that can return a cache key.
    /// </summary>
    public interface ICacheable
    {
        /// <summary>
        /// Returns a cache key.
        /// </summary>
        string GetCacheKey();
    }
}
