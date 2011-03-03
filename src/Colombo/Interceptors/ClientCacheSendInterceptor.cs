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
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Castle.Core.Logging;
using Colombo.Caching;

namespace Colombo.Interceptors
{
    /// <summary>
    /// Interceptor that consult the cache before sending and returns a response directly if there is a match.
    /// </summary>
    /// <see cref="EnableCacheAttribute"/>
    /// <see cref="CacheSegmentAttribute"/>
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
        /// Check requests marked with a <see cref="EnableCacheAttribute"/> attribute for the cache.
        /// The segment is determined by the <see cref="CacheSegmentAttribute"/> attribute if any, otherwise <c>null</c> will be use.
        /// </summary>
        public void Intercept(IColomboSendInvocation nextInvocation)
        {
            if (nextInvocation == null) throw new ArgumentNullException("nextInvocation");
            Contract.EndContractBlock();

            var requestsToDelete = new List<BaseRequest>();
            var responsesGroup = new ResponsesGroup();
            foreach (var request in nextInvocation.Requests)
            {
                var enableClientCachingAttribute = request.GetCustomAttribute<EnableCacheAttribute>();
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
