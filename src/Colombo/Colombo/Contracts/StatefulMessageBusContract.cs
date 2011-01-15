using System;
using System.Diagnostics.Contracts;

namespace Colombo.Contracts
{
#pragma warning disable 1591 // docs
    [ContractClassFor(typeof(IStatefulMessageBus))]
    public abstract class StatefulMessageBusContract : IStatefulMessageBus
    {
        public TResponse FutureSend<TResponse>(SideEffectFreeRequest<TResponse> request)
            where TResponse : Response, new()
        {
            Contract.Requires<ArgumentNullException>(request != null, "request");
            Contract.Ensures(Contract.Result<TResponse>() != null);
            throw new NotImplementedException();
        }

        public int NumberOfSend
        {
            get { throw new NotImplementedException(); }
        }

        public int MaxAllowedNumberOfSend
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public TResponse Send<TResponse>(Request<TResponse> request)
            where TResponse : Response, new()
        {
            throw new NotImplementedException();
        }

        public IAsyncCallback<TResponse> SendAsync<TResponse>(Request<TResponse> request)
            where TResponse : Response, new()
        {
            throw new NotImplementedException();
        }

        public TResponse Send<TResponse>(SideEffectFreeRequest<TResponse> request)
            where TResponse : Response, new()
        {
            throw new NotImplementedException();
        }

        public TResponse Send<TRequest, TResponse>(Action<TRequest> action)
            where TRequest : SideEffectFreeRequest<TResponse>, new()
            where TResponse : Response, new()
        {
            throw new NotImplementedException();
        }

        public IAsyncCallback<TResponse> SendAsync<TResponse>(SideEffectFreeRequest<TResponse> request)
            where TResponse : Response, new()
        {
            throw new NotImplementedException();
        }

        public ResponsesGroup Send(BaseSideEffectFreeRequest request, params BaseSideEffectFreeRequest[] followingRequests)
        {
            throw new NotImplementedException();
        }

        public void Notify(Notification notification, params Notification[] notifications)
        {
            throw new NotImplementedException();
        }
    }
#pragma warning restore 1591
}
