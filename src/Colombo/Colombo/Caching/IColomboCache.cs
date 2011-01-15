using System;
using System.Diagnostics.Contracts;

namespace Colombo.Caching
{
    [ContractClass(typeof(Contracts.ColomboCacheContract))]
    public interface IColomboCache
    {
        void Store(string segment, string cacheKey, object @object, TimeSpan duration);
        object Get(string segment, Type objectType, string cacheKey);

        void Flush(string segment, Type objectType);
        void FlushAll();
    }
}
