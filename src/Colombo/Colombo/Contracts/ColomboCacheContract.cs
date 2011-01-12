using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Colombo.Caching;
using System.Diagnostics.Contracts;

namespace Colombo.Contracts
{
    [ContractClassFor(typeof(IColomboCache))]
    public abstract class ColomboCacheContract : IColomboCache
    {
        public void Store(string segment, string cacheKey, object @object, TimeSpan duration)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(cacheKey), "cacheKey");
            Contract.Requires<ArgumentNullException>(@object != null, "object");
            Contract.Requires<ArgumentException>(!duration.Equals(TimeSpan.MaxValue), "duration");
            throw new NotImplementedException();
        }

        public object Get(string segment, Type objectType, string cacheKey)
        {
            Contract.Requires<ArgumentNullException>(objectType != null);
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(cacheKey), "cacheKey");
            throw new NotImplementedException();
        }

        public void InvalidateAllObjects(string segment, Type objectType)
        {
            throw new NotImplementedException();
        }
    }
}
