using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Colombo.Caching;
using System.Diagnostics.Contracts;

namespace Colombo.Contracts
{
    [ContractClassFor(typeof(ICache))]
    public abstract class CacheContract : ICache
    {
        public void Store(string segment, string cacheKey, object @object, TimeSpan duration)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(cacheKey), "cacheKey");
            Contract.Requires<ArgumentNullException>(@object != null, "object");
            Contract.Requires<ArgumentException>(!duration.Equals(TimeSpan.MaxValue), "duration");
            throw new NotImplementedException();
        }

        public T Get<T>(string segment, string cacheKey) where T : class
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(cacheKey), "cacheKey");
            throw new NotImplementedException();
        }

        public void InvalidateAllObjects<T>(string segment)
        {
            throw new NotImplementedException();
        }

        public void InvalidateAllObjects(string segment, Type t)
        {
            throw new NotImplementedException();
        }
    }
}
