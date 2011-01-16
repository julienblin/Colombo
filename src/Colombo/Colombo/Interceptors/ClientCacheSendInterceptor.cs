using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Castle.Core.Logging;
using Colombo.Caching;

namespace Colombo.Interceptors
{
    /// <summary>
    /// Interceptor that puts request in the cache when possible, and also invalidates them.
    /// </summary>
    /// <see cref="EnableClientCachingAttribute"/>
    /// <see cref="CacheSegmentAttribute"/>
    /// <see cref="InvalidateCachedInstancesOfAttribute"/>
    public class ClientCacheSendInterceptor : IMessageBusSendInterceptor
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
        public ClientCacheSendInterceptor(IColomboCache cache)
        {
            if (cache == null) throw new ArgumentNullException("cache");
            Contract.EndContractBlock();

            this.cache = cache;
        }

        /// <summary>
        /// Puts request marked with a <see cref="EnableClientCachingAttribute"/> attribute in the cache.
        /// The segment is determined by the <see cref="CacheSegmentAttribute"/> attribute if any, otherwise <c>null</c> will be use.
        /// It also invalidates the cache using <see cref="InvalidateCachedInstancesOfAttribute"/>.
        /// </summary>
        public void Intercept(IColomboSendInvocation nextInvocation)
        {
            if (nextInvocation == null) throw new ArgumentNullException("nextInvocation");
            Contract.EndContractBlock();

            var requestsToDelete = new List<BaseRequest>();
            var responsesGroup = new ResponsesGroup();
            foreach (var request in nextInvocation.Requests)
            {
                var invalidateCachedInstancesOfAttributes = request.GetCustomAttributes<InvalidateCachedInstancesOfAttribute>();
                foreach (var invalidateCachedInstancesOfAttribute in invalidateCachedInstancesOfAttributes)
                {
                    foreach (var responseType in invalidateCachedInstancesOfAttribute.ResponsesTypes)
                    {
                        string cacheSegmentForCache = null;
                        var cacheSegmentAttributeForCache = request.GetCustomAttribute<CacheSegmentAttribute>();
                        if (cacheSegmentAttributeForCache != null)
                            cacheSegmentForCache = cacheSegmentAttributeForCache.GetCacheSegment(request);

                        Logger.DebugFormat("Invalidating all responses of type {0} from cache segment {1}", responseType, cacheSegmentForCache);
                        cache.Flush(cacheSegmentForCache, responseType);
                    }
                }

                var enableClientCachingAttribute = request.GetCustomAttribute<EnableClientCachingAttribute>();
                if (enableClientCachingAttribute == null) continue;

                var cacheKey = request.GetCacheKey();

                string cacheSegment = null;
                var cacheSegmentAttribute = request.GetCustomAttribute<CacheSegmentAttribute>();
                if (cacheSegmentAttribute != null)
                    cacheSegment = cacheSegmentAttribute.GetCacheSegment(request);

                Logger.DebugFormat("Testing cache for {0}: segment {1} - cacheKey: {2}", request, cacheSegment, cacheKey);
                var retrievedFromCache = (Response)cache.Get(cacheSegment, request.GetResponseType(), cacheKey);
                if (retrievedFromCache == null) continue;

                Logger.DebugFormat("Cache hit for cacheKey {0}: responding with {1}", cacheKey, retrievedFromCache);
                responsesGroup[request] = retrievedFromCache;
                requestsToDelete.Add(request);
            }

            foreach (var request in requestsToDelete)
                nextInvocation.Requests.Remove(request);

            nextInvocation.Proceed();

            foreach (var request in nextInvocation.Requests)
            {
                var enableClientCachingAttribute = request.GetCustomAttribute<EnableClientCachingAttribute>();
                if (enableClientCachingAttribute == null) continue;

                var cacheKey = request.GetCacheKey();

                string cacheSegment = null;
                var cacheSegmentAttribute = request.GetCustomAttribute<CacheSegmentAttribute>();
                if (cacheSegmentAttribute != null)
                    cacheSegment = cacheSegmentAttribute.GetCacheSegment(request);

                Logger.DebugFormat("Caching {0} in segment {1} with cacheKey {2} for {3}", request, cacheSegment, cacheKey, enableClientCachingAttribute.Duration);
                cache.Store(cacheSegment, cacheKey, nextInvocation.Responses[request], enableClientCachingAttribute.Duration);
            }

            if (responsesGroup.Count <= 0) return;

            if (nextInvocation.Responses == null)
                nextInvocation.Responses = new ResponsesGroup();

            foreach (var item in responsesGroup)
            {
                nextInvocation.Responses.Add(item.Key, item.Value);
            }
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
