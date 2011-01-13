using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Castle.DynamicProxy;
using Colombo.Impl.Async;

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

            CheckNumberOfSend(false);

            var options = new ProxyGenerationOptions(new NonVirtualCheckProxyGenerationHook());
            var response = proxyGenerator.CreateClassProxy<TResponse>(options, new StatefulReponseInterceptor(this, request));
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

        int numberOfSend = 0;

        public int NumberOfSend
        {
            get { return numberOfSend; }
        }

        public int MaxAllowedNumberOfSend { get; set; }

        public TResponse Send<TResponse>(Request<TResponse> request)
            where TResponse : Response, new()
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();
            CheckNumberOfSend();
            return messageBus.Send(request);
        }

        public IAsyncCallback<TResponse> SendAsync<TResponse>(Request<TResponse> request)
            where TResponse : Response, new()
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();
            CheckNumberOfSend();
            return messageBus.SendAsync(request);
        }

        public TResponse Send<TResponse>(SideEffectFreeRequest<TResponse> request)
            where TResponse : Response, new()
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();
            CheckNumberOfSend();
            return messageBus.Send(request);
        }

        public IAsyncCallback<TResponse> SendAsync<TResponse>(SideEffectFreeRequest<TResponse> request)
            where TResponse : Response, new()
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();
            CheckNumberOfSend();
            return messageBus.SendAsync(request);
        }

        public ResponsesGroup Send(BaseSideEffectFreeRequest request, params BaseSideEffectFreeRequest[] followingRequests)
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();
            CheckNumberOfSend();
            return messageBus.Send(request, followingRequests);
        }

        public void Notify(Notification notification, params Notification[] notifications)
        {
            if (notification == null) throw new ArgumentNullException("notification");
            Contract.EndContractBlock();

            messageBus.Notify(notification, notifications);
        }

        private void CheckNumberOfSend(bool increment = true)
        {
            if ((MaxAllowedNumberOfSend > 0) && (NumberOfSend >= MaxAllowedNumberOfSend))
                throw new ColomboException(string.Format("StatefulMessageBus has already sent {0} batches of request, you cannot send more. Change MaxAllowedNumberOfSend for this instance or MaxAllowedNumberOfSendForStatefulMessageBus in facility configuration to disable this behavior (currently: {1}).", NumberOfSend, MaxAllowedNumberOfSend));

            if (increment)
                ++numberOfSend;
        }
    }
}
