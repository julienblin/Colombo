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
using System.Linq;
using Castle.DynamicProxy;
using Colombo.Impl.Async;

namespace Colombo.Impl
{
    /// <summary>
    /// Default implementation of <see cref="IStatefulMessageBus"/> that uses an <see cref="IMessageBus"/> to send.
    /// </summary>
    public class StatefulMessageBus : IStatefulMessageBus
    {
        private static readonly ProxyGenerator ProxyGenerator = new ProxyGenerator();

        private readonly IMessageBus messageBus;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="messageBus">The inner message bus to use.</param>
        public StatefulMessageBus(IMessageBus messageBus)
        {
            if (messageBus == null) throw new ArgumentNullException("messageBus");
            Contract.EndContractBlock();

            this.messageBus = messageBus;
        }

        /// <summary>
        /// Return a promise of response - that is a proxy that when accessed the first time, it will send.
        /// This allow the batching of several FutureSend together.
        /// </summary>
        public TResponse FutureSend<TResponse>(SideEffectFreeRequest<TResponse> request)
            where TResponse : Response, new()
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();

            CheckNumberOfSend(false);

            var options = new ProxyGenerationOptions(new NonVirtualCheckProxyGenerationHook());
            var response = ProxyGenerator.CreateClassProxy<TResponse>(options, new StatefulReponseInterceptor(this, request));
            PendingRequests.Add(request);

            Contract.Assume(response != null);
            return response;
        }

        private IList<BaseSideEffectFreeRequest> pendingRequests;
        private IList<BaseSideEffectFreeRequest> PendingRequests
        {
            get { return pendingRequests ?? (pendingRequests = new List<BaseSideEffectFreeRequest>()); }
        }

        private IDictionary<BaseSideEffectFreeRequest, Response> receivedRequests;
        private IDictionary<BaseSideEffectFreeRequest, Response> ReceivedRequests
        {
            get { return receivedRequests ?? (receivedRequests = new Dictionary<BaseSideEffectFreeRequest, Response>()); }
        }

        internal Response GetResponseForPendingRequest(BaseSideEffectFreeRequest request)
        {
            if (ReceivedRequests.ContainsKey(request))
                return ReceivedRequests[request];

            if (!PendingRequests.Contains(request))
                throw new ColomboException(string.Format("Internal error: response proxy request is no registered with this StatefulMessageBus instance: {0}", request));

            var firstPendingRequest = PendingRequests[0];
            PendingRequests.Remove(firstPendingRequest);

            Contract.Assume(firstPendingRequest != null);

            CheckNumberOfSend();
            var responsesGroups = messageBus.Send(firstPendingRequest, PendingRequests.ToArray());
            PendingRequests.Clear();

            ReceivedRequests[firstPendingRequest] = responsesGroups[firstPendingRequest];
            foreach (var responsesGroup in responsesGroups)
            {
                ReceivedRequests[(BaseSideEffectFreeRequest)responsesGroup.Key] = responsesGroup.Value;
            }

            return ReceivedRequests[request];
        }

        /// <summary>
        /// The number of time this <see cref="IStatefulMessageBus"/> has already sent.
        /// </summary>
        public int NumberOfSend { get; private set; }

        /// <summary>
        /// The maximum allowed number of send that this <see cref="IStatefulMessageBus"/> will allow.
        /// After this quota, every attempt to send will result in a <see cref="ColomboException"/>.
        /// </summary>
        public int MaxAllowedNumberOfSend { get; set; }

        /// <summary>
        /// Send synchronously a request and returns the response.
        /// </summary>
        public Response Send(BaseRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();
            CheckNumberOfSend();
            return messageBus.Send(request);
        }

        /// <summary>
        /// Send synchronously a request and returns the response.
        /// </summary>
        public TResponse Send<TResponse>(Request<TResponse> request)
            where TResponse : Response, new()
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();
            CheckNumberOfSend();
            return messageBus.Send(request);
        }

        /// <summary>
        /// Send a request asynchronously. You must register a callback with the result to get the response or the error.
        /// </summary>
        public IAsyncCallback<TResponse> SendAsync<TResponse>(Request<TResponse> request)
            where TResponse : Response, new()
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();
            CheckNumberOfSend();
            return messageBus.SendAsync(request);
        }

        /// <summary>
        /// Send synchronously a request and returns the response.
        /// </summary>
        public TResponse Send<TResponse>(SideEffectFreeRequest<TResponse> request)
            where TResponse : Response, new()
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();
            CheckNumberOfSend();
            return messageBus.Send(request);
        }

        /// <summary>
        /// Send synchronously a request and returns the response.
        /// </summary>
        public TResponse Send<TRequest, TResponse>(Action<TRequest> action)
            where TRequest : SideEffectFreeRequest<TResponse>, new()
            where TResponse : Response, new()
        {
            if (action == null) throw new ArgumentNullException("action");
            Contract.EndContractBlock();
            CheckNumberOfSend();
            return messageBus.Send<TRequest, TResponse>(action);
        }

        /// <summary>
        /// Send a request asynchronously. You must register a callback with the result to get the response or the error.
        /// </summary>
        public IAsyncCallback<TResponse> SendAsync<TResponse>(SideEffectFreeRequest<TResponse> request)
            where TResponse : Response, new()
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();
            CheckNumberOfSend();
            return messageBus.SendAsync(request);
        }

        /// <summary>
        /// Send synchronously, but in parallel, several requests and returns all the responses at once.
        /// Only side effect-free requests can be parallelized.
        /// </summary>
        public ResponsesGroup Send(BaseSideEffectFreeRequest request, params BaseSideEffectFreeRequest[] followingRequests)
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();
            CheckNumberOfSend();
            return messageBus.Send(request, followingRequests);
        }

        private void CheckNumberOfSend(bool increment = true)
        {
            if ((MaxAllowedNumberOfSend > 0) && (NumberOfSend >= MaxAllowedNumberOfSend))
                throw new ColomboException(string.Format("StatefulMessageBus has already sent {0} batches of request, you cannot send more. Change MaxAllowedNumberOfSend for this instance or MaxAllowedNumberOfSendForStatefulMessageBus in facility configuration to disable this behavior (currently: {1}).", NumberOfSend, MaxAllowedNumberOfSend));

            if (increment)
                ++NumberOfSend;
        }
    }
}
