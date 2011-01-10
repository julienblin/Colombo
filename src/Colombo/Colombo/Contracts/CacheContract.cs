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
        public void Store(string cacheKey, object @object, TimeSpan duration)
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(cacheKey), "cacheKey");
            Contract.Requires<ArgumentNullException>(@object != null, "object");
            Contract.Requires<ArgumentException>(!duration.Equals(TimeSpan.MaxValue), "duration");
            throw new NotImplementedException();
        }

        public T Get<T>(string cacheKey) where T : class
        {
            Contract.Requires<ArgumentNullException>(!string.IsNullOrEmpty(cacheKey), "cacheKey");
            throw new NotImplementedException();
        }

        public string Segment
        {
            get { throw new NotImplementedException(); }
        }
    }
}
