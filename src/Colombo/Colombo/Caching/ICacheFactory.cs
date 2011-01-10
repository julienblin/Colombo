using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Colombo.Caching
{
    public interface ICacheFactory
    {
        ICache GetCacheForSegment(string segment);
    }
}
