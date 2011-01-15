using System;
using System.Diagnostics.Contracts;

namespace Colombo
{
    /// <summary>
    /// Use this base class to create request handlers for side effect-free requests.
    /// </summary>
    public abstract class SideEffectFreeRequestHandler<TRequest, TResponse> : ISideEffectFreeRequestHandler<TRequest, TResponse>
        where TResponse : Response, new()
        where TRequest : SideEffectFreeRequest<TResponse>, new()
    {
        private TRequest request;
        protected TRequest Request { get { return request; } }

        private TResponse response;
        protected TResponse Response { get { return response; } }

        public Response Handle(BaseRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();

            return Handle((TRequest)request);
        }

        public TResponse Handle(TRequest request)
        {
            this.request = request;
            this.response = new TResponse();
            response.CorrelationGuid = request.CorrelationGuid;
            Handle();

            Contract.Assume(response != null);
            return response;
        }

        public abstract void Handle();

        protected TNewRequest CreateRequest<TNewRequest>()
            where TNewRequest : BaseRequest, new()
        {
            var result = new TNewRequest();
            result.CorrelationGuid = Request.CorrelationGuid;
            result.Context = Request.Context;
            return result;
        }

        protected TNotification CreateNotification<TNotification>()
            where TNotification : Notification, new()
        {
            var result = new TNotification();
            result.CorrelationGuid = Request.CorrelationGuid;
            result.Context = Request.Context;
            return result;
        }

        public Type GetRequestType()
        {
            return typeof(TRequest);
        }

        public Type GetResponseType()
        {
            return typeof(TResponse);
        }
    }
}
