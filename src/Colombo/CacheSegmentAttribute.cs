using System;

namespace Colombo
{
    /// <summary>
    /// Specify the cache segment to use. Must be applied on a request.
    /// The segment can be defined by either a name or a pointer to a value in the Context.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class CacheSegmentAttribute : Attribute
    {
        /// <summary>
        /// The name of the cache segment to use.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The name of the ContextKey which value will be used for the cache segment.
        /// If no value is in the Context and a <see cref="Name"/> is provided, the Name will be used instead.
        /// </summary>
        public string FromContextKey { get; set; }

        /// <summary>
        /// Specify the cache segment to use. Must be applied on a request.
        /// </summary>
        public CacheSegmentAttribute()
        {
        }

        /// <summary>
        /// Specify the cache segment to use. Must be applied on a request.
        /// </summary>
        /// <param name="name">Name of the cache segment.</param>
        public CacheSegmentAttribute(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Returns the cache segment, specified either by the <see cref="FromContextKey"/> or <see cref="Name"/> properties.
        /// The FromContextKey has priority over the Name.
        /// </summary>
        public string GetCacheSegment(BaseRequest request)
        {
            if (string.IsNullOrEmpty(Name) && string.IsNullOrEmpty(FromContextKey))
                throw new ColomboException(string.Format("Unable to determine cache segment for {0}. You must specified either the Name or the FromContextKey member.", request));

            if (!string.IsNullOrEmpty(FromContextKey))
            {
                if (request.Context.ContainsKey(FromContextKey))
                    return request.Context[FromContextKey];

                if (string.IsNullOrEmpty(Name))
                    throw new ColomboException(string.Format("The cache segment for {0} is supposed to come from the context key {1}, but it doesn't exists on Context and no Name has been given.",
                        request,
                        FromContextKey));
            }

            return Name;
        }
    }
}
