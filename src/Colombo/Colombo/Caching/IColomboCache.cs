using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Colombo.Caching
{
    [ContractClass(typeof(Contracts.ColomboCacheContract))]
    public interface IColomboCache
    {
        void Store(string segment, string cacheKey, object @object, TimeSpan duration);
        object Get(string segment, Type objectType, string cacheKey);

        void InvalidateAllObjects(string segment, Type objectType);
    }
}
