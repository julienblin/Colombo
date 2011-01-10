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
        void Store(string cacheKey, object @object, TimeSpan duration);
        T Get<T>(string cacheKey) where T : class;

        void InvalidateAllObjects<T>();
        void InvalidateAllObjects(Type t);

        string Segment { get; set; }
    }
}
