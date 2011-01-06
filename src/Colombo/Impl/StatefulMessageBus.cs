using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Castle.DynamicProxy;

namespace Colombo.Impl
{
    public class StatefulMessageBus : IStatefulMessageBus
    {
        private static readonly ProxyGenerator proxyGenerator = new ProxyGenerator();

        private readonly IMessageBus messageBus;

        public StatefulMessageBus(IMessageBus messageBus)
        {
            if (messageBus == null) throw new ArgumentNullException("messageBus");
            Contract.EndContractBlock();

            this.messageBus = messageBus;
        }

        public TResponse FutureSend<TResponse>(SideEffectFreeRequest<TResponse> request)
            where TResponse : Response, new()
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();

            if (hasAlreadySentForFutures && !AllowMultipleFutureSendBatches)
                throw new ColomboException("StatefulMessageBus has already sent a batch of request, you cannot queue more. Change AllowMultipleFutureSendBatches for this instance or in facility configuration to disable this behavior.");

            var response = proxyGenerator.CreateClassProxy<TResponse>(new StatefulReponseInterceptor(this, request));
            PendingRequests.Add(request);

            Contract.Assume(response != null);
            return response;
        }

        private IList<BaseSideEffectFreeRequest> pendingRequests;
        private IList<BaseSideEffectFreeRequest> PendingRequests
        {
            get
            {
                if (pendingRequests == null)
                    pendingRequests = new List<BaseSideEffectFreeRequest>();
                return pendingRequests;
            }
        }

        private IDictionary<BaseSideEffectFreeRequest, Response> receivedRequests;
        private IDictionary<BaseSideEffectFreeRequest, Response> ReceivedRequests
        {
            get
            {
                if (receivedRequests == null)
                    receivedRequests = new Dictionary<BaseSideEffectFreeRequest, Response>();
                return receivedRequests;
            }
        }

        public Response GetResponseForPendingRequest(BaseSideEffectFreeRequest request)
        {
            if (ReceivedRequests.ContainsKey(request))
                return ReceivedRequests[request];

            if (!PendingRequests.Contains(request))
                return null;

            if (PendingRequests.Count == 0)
                return null;

            var firstPendingRequest = PendingRequests[0];
            PendingRequests.Remove(firstPendingRequest);

            Contract.Assume(firstPendingRequest != null);
            var responsesGroups = messageBus.Send(firstPendingRequest, PendingRequests.ToArray());
            hasAlreadySentForFutures = true;
            PendingRequests.Clear();

            ReceivedRequests[firstPendingRequest] = responsesGroups[firstPendingRequest];
            foreach (var responsesGroup in responsesGroups)
            {
                ReceivedRequests[(BaseSideEffectFreeRequest)responsesGroup.Key] = responsesGroup.Value;
            }

            return ReceivedRequests[request];
        }

        bool hasAlreadySentForFutures = false;

        public bool HasAlreadySentForFutures
        {
            get { return hasAlreadySentForFutures; }
        }

        public bool AllowMultipleFutureSendBatches { get; set; }

        public TResponse Send<TResponse>(Request<TResponse> request)
            where TResponse : Response, new()
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();
            return messageBus.Send(request);
        }

        public TResponse Send<TResponse>(SideEffectFreeRequest<TResponse> request)
            where TResponse : Response, new()
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();
            return messageBus.Send(request);
        }

        public ResponsesGroup Send(BaseSideEffectFreeRequest request, params BaseSideEffectFreeRequest[] followingRequests)
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();
            return messageBus.Send(request, followingRequests);
        }
    }
}
