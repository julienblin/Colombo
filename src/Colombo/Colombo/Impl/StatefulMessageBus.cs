using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Castle.DynamicProxy;
using Colombo.Impl.Async;

namespace Colombo.Impl
{
    public class StatefulMessageBus : IStatefulMessageBus
    {
        private static readonly ProxyGenerator ProxyGenerator = new ProxyGenerator();

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

        public int NumberOfSend { get; private set; }

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

        public TResponse Send<TRequest, TResponse>(Action<TRequest> action)
            where TRequest : SideEffectFreeRequest<TResponse>, new()
            where TResponse : Response, new()
        {
            if (action == null) throw new ArgumentNullException("action");
            Contract.EndContractBlock();
            CheckNumberOfSend();
            return messageBus.Send<TRequest, TResponse>(action);
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
                ++NumberOfSend;
        }
    }
}
