using System;

namespace Colombo
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class CacheSegmentAttribute : Attribute
    {
        public string Name { get; set; }
        public string FromContextKey { get; set; }

        public CacheSegmentAttribute()
        {
        }

        public CacheSegmentAttribute(string name)
        {
            Name = name;
        }

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
