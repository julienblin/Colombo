using System;
using System.Diagnostics.Contracts;
using Castle.Core.Logging;
using Colombo.Caching;

namespace Colombo.Interceptors
{
    /// <summary>
    /// Interceptor used server-side to invalidate objects from the cache.
    /// It does not put them in the cache, just invalidate the values.
    /// </summary>
    /// <see cref="InvalidateCachedInstancesOfAttribute"/>
    public class CacheHandleInterceptor : IRequestHandlerHandleInterceptor
    {
        private ILogger logger = NullLogger.Instance;
        /// <summary>
        /// Logger
        /// </summary>
        public ILogger Logger
        {
            get { return logger; }
            set { logger = value; }
        }

        private readonly IColomboCache cache;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="cache">The cache to use</param>
        public CacheHandleInterceptor(IColomboCache cache)
        {
            if (cache == null) throw new ArgumentNullException("cache");
            Contract.EndContractBlock();

            this.cache = cache;
        }

        /// <summary>
        /// Performs the invalidation if the request has an <see cref="InvalidateCachedInstancesOfAttribute"/> attribute.
        /// </summary>
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

        /// <summary>
        /// Runs last in the invocation chain.
        /// </summary>
        public int InterceptionPriority
        {
            get { return InterceptionPrority.ReservedLow; }
        }
    }
}
