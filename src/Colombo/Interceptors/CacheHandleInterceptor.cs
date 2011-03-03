#region License
// The MIT License
// 
// Copyright (c) 2011 Julien Blin, julien.blin@gmail.com
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
#endregion

using System;
using System.Diagnostics.Contracts;
using Castle.Core.Logging;
using Colombo.Caching;

namespace Colombo.Interceptors
{
    /// <summary>
    /// Interceptor used server-side to put objects in cache and invalidate objects from the cache.
    /// </summary>
    /// <see cref="EnableCacheAttribute"/>
    /// <see cref="CacheSegmentAttribute"/>
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

            var enableClientCachingAttribute = nextInvocation.Request.GetCustomAttribute<EnableCacheAttribute>();
            if (enableClientCachingAttribute == null) return;

            var cacheKey = nextInvocation.Request.GetCacheKey();

            string cacheSegmentAfter = null;
            var cacheSegmentAttributeAfter = nextInvocation.Request.GetCustomAttribute<CacheSegmentAttribute>();
            if (cacheSegmentAttributeAfter != null)
                cacheSegmentAfter = cacheSegmentAttributeAfter.GetCacheSegment(nextInvocation.Request);

            Logger.DebugFormat("Caching {0} in segment {1} with cacheKey {2} for {3}", nextInvocation.Request, cacheSegmentAfter, cacheKey, enableClientCachingAttribute.Duration);
            cache.Store(cacheSegmentAfter, cacheKey, nextInvocation.Response, enableClientCachingAttribute.Duration);
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
