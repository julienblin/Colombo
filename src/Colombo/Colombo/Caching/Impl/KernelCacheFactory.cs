using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.MicroKernel;
using System.Collections.Concurrent;
using Castle.Core.Logging;
using System.Diagnostics.Contracts;

namespace Colombo.Caching.Impl
{
    public class KernelCacheFactory : ICacheFactory
    {
        private ILogger logger = NullLogger.Instance;
        public ILogger Logger
        {
            get { return logger; }
            set { logger = value; }
        }

        private readonly IKernel kernel;

        public KernelCacheFactory(IKernel kernel)
        {
            if (kernel == null) throw new ArgumentNullException("kernel");
            Contract.EndContractBlock();

            this.kernel = kernel;
        }

        private ConcurrentDictionary<string, ICache> cachesPerSegment = new ConcurrentDictionary<string, ICache>();

        public ICache GetCacheForSegment(string segment)
        {
            try
            {
                return cachesPerSegment.GetOrAdd(segment, (s) =>
                {
                    var cache = kernel.Resolve<ICache>();
                    cache.Segment = s;
                    return cache;
                });
            }
            catch (ComponentNotFoundException ex)
            {
                var errorMessage = string.Format("Unable to create an ICache for segment {0}. Did you forget to register it on the container? If so, don't forget also that it has to be Transient.", segment);
                Logger.Error(errorMessage);
                throw new ColomboException(errorMessage, ex);
            }
        }
    }
}
