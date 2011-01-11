using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Colombo.Caching
{
    [ContractClass(typeof(Contracts.CacheContract))]
    public interface ICache
    {
        void Store(string segment, string cacheKey, object @object, TimeSpan duration);
        T Get<T>(string segment, string cacheKey, Type cacheType) where T : class;

        void InvalidateAllObjects(string segment, Type cacheType);
    }
}
