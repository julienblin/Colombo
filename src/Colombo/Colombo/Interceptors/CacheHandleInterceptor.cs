using System;
using System.Diagnostics.Contracts;
using Castle.Core.Logging;
using Colombo.Caching;

namespace Colombo.Interceptors
{
    public class CacheHandleInterceptor : IRequestHandlerHandleInterceptor
    {
        private ILogger logger = NullLogger.Instance;
        public ILogger Logger
        {
            get { return logger; }
            set { logger = value; }
        }

        private readonly IColomboCache cache;

        public CacheHandleInterceptor(IColomboCache cache)
        {
            if (cache == null) throw new ArgumentNullException("cache");
            Contract.EndContractBlock();

            this.cache = cache;
        }

        public void Intercept(IColomboRequestHandleInvocation nextInvocation)
        {
            var invalidateCachedInstancesOfAttributes = nextInvocation.Request.GetCustomAttributes<InvalidateCachedInstancesOfAttribute>();
            foreach (var invalidateCachedInstancesOfAttribute in invalidateCachedInstancesOfAttributes)
            {
                foreach (var responseType in invalidateCachedInstancesOfAttribute.ResponsesTypes)
                {
                    string cacheSegment = null;
                    var cacheSegmentAttribute = nextInvocation.Request.GetCustomAttribute<CacheSegmentAttribute>();
                    if (cacheSegmentAttribute != null)
                        cacheSegment = cacheSegmentAttribute.GetCacheSegment(nextInvocation.Request);

                    Logger.DebugFormat("Invalidating all responses of type {0} from cache segment {1}", responseType, cacheSegment);
                    cache.Flush(cacheSegment, responseType);
                }
            }

            nextInvocation.Proceed();
        }

        public int InterceptionPriority
        {
            get { return InterceptorPrority.ReservedLow; }
        }
    }
}
