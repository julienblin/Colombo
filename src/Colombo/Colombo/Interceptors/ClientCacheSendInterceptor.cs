using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Colombo.Caching;
using Castle.Core.Logging;

namespace Colombo.Interceptors
{
    public class ClientCacheSendInterceptor : IMessageBusSendInterceptor
    {
        private ILogger logger = NullLogger.Instance;
        public ILogger Logger
        {
            get { return logger; }
            set { logger = value; }
        }

        private readonly ICacheFactory cacheFactory;

        public ClientCacheSendInterceptor(ICacheFactory cacheFactory)
        {
            if (cacheFactory == null) throw new ArgumentNullException("cacheFactory");
            Contract.EndContractBlock();

            this.cacheFactory = cacheFactory;
        }

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
                        string cacheSegment = null;
                        var cacheSegmentAttribute = request.GetCustomAttribute<CacheSegmentAttribute>();
                        if (cacheSegmentAttribute != null)
                            cacheSegment = cacheSegmentAttribute.GetCacheSegment(request);

                        Logger.DebugFormat("Invalidating all responses of type {0} from cache segment {1}", responseType, cacheSegment);
                        cacheFactory.GetCacheForSegment(cacheSegment).InvalidateAllObjects(responseType);
                    }
                }

                var enableClientCachingAttribute = request.GetCustomAttribute<EnableClientCachingAttribute>();
                if (enableClientCachingAttribute != null)
                {
                    var cacheKey = request.GetCacheKey();

                    string cacheSegment = null;
                    var cacheSegmentAttribute = request.GetCustomAttribute<CacheSegmentAttribute>();
                    if (cacheSegmentAttribute != null)
                        cacheSegment = cacheSegmentAttribute.GetCacheSegment(request);

                    Logger.DebugFormat("Testing cache for {0}: segment {1} - cacheKey: {2}", request, cacheSegment, cacheKey);
                    var retrievedFromCache = cacheFactory.GetCacheForSegment(cacheSegment).Get<Response>(cacheKey);
                    if (retrievedFromCache != null)
                    {
                        Logger.DebugFormat("Cache hit for cacheKey {0}: responding with {1}", cacheKey, retrievedFromCache);
                        responsesGroup[request] = retrievedFromCache;
                        requestsToDelete.Add(request);
                    }
                }
            }

            foreach (var request in requestsToDelete)
                nextInvocation.Requests.Remove(request);

            nextInvocation.Proceed();

            foreach (var request in nextInvocation.Requests)
            {
                var enableClientCachingAttribute = request.GetCustomAttribute<EnableClientCachingAttribute>();
                if (enableClientCachingAttribute != null)
                {
                    var cacheKey = request.GetCacheKey();

                    string cacheSegment = null;
                    var cacheSegmentAttribute = request.GetCustomAttribute<CacheSegmentAttribute>();
                    if (cacheSegmentAttribute != null)
                        cacheSegment = cacheSegmentAttribute.GetCacheSegment(request);

                    Logger.DebugFormat("Caching {0} in segment {1} with cacheKey {2} for {3}", request, cacheSegment, cacheKey, enableClientCachingAttribute.Duration);
                    cacheFactory.GetCacheForSegment(cacheSegment).Store(cacheKey, nextInvocation.Responses[request], enableClientCachingAttribute.Duration);
                }
            }

            if (responsesGroup.Count > 0)
            {
                if (nextInvocation.Responses == null)
                    nextInvocation.Responses = new ResponsesGroup();

                foreach (var item in responsesGroup)
                {
                    nextInvocation.Responses.Add(item.Key, item.Value);
                }
            }
        }

        public int InterceptionPriority
        {
            get { return InterceptorPrority.ReservedLow; }
        }
    }
}
