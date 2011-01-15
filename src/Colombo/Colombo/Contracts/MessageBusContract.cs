using System;
using System.Diagnostics.Contracts;

namespace Colombo.Contracts
{
#pragma warning disable 1591 // docs
    [ContractClassFor(typeof(IMessageBus))]
    public abstract class MessageBusContract : IMessageBus
    {
        public TResponse Send<TResponse>(Request<TResponse> request)
            where TResponse : Response, new()
        {
            Contract.Requires<ArgumentNullException>(request != null, "request");
            Contract.Ensures(Contract.Result<TResponse>() != null);
            throw new NotImplementedException();
        }

        public IAsyncCallback<TResponse> SendAsync<TResponse>(Request<TResponse> request)
            where TResponse : Response, new()
        {
            Contract.Requires<ArgumentNullException>(request != null, "request");
            Contract.Ensures(Contract.Result<IAsyncCallback<TResponse>>() != null);
            throw new NotImplementedException();
        }

        public TResponse Send<TResponse>(SideEffectFreeRequest<TResponse> request)
            where TResponse : Response, new()
        {
            Contract.Requires<ArgumentNullException>(request != null, "request");
            Contract.Ensures(Contract.Result<TResponse>() != null);
            throw new NotImplementedException();
        }

        public TResponse Send<TRequest, TResponse>(Action<TRequest> action)
            where TRequest : SideEffectFreeRequest<TResponse>, new()
            where TResponse : Response, new()
        {
            Contract.Requires<ArgumentNullException>(action != null, "action");
            Contract.Ensures(Contract.Result<TResponse>() != null);
            throw new NotImplementedException();
        }

        public IAsyncCallback<TResponse> SendAsync<TResponse>(SideEffectFreeRequest<TResponse> request)
            where TResponse : Response, new()
        {
            Contract.Requires<ArgumentNullException>(request != null, "request");
            Contract.Ensures(Contract.Result<IAsyncCallback<TResponse>>() != null);
            throw new NotImplementedException();
        }

        public ResponsesGroup Send(BaseSideEffectFreeRequest request, params BaseSideEffectFreeRequest[] followingRequests)
        {
            Contract.Requires<ArgumentNullException>(request != null, "request");
            Contract.Ensures(Contract.Result<ResponsesGroup>() != null);
            throw new NotImplementedException();
        }

        public void Notify(Notification notification, params Notification[] notifications)
        {
            Contract.Requires<ArgumentNullException>(notification != null, "notification");
            throw new NotImplementedException();
        }
    }
#pragma warning restore 1591
}
