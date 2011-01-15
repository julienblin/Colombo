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
        protected TRequest Request { get; private set; }

        protected TResponse Response { get; private set; }

        public Response Handle(BaseRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();

            return Handle((TRequest)request);
        }

        public TResponse Handle(TRequest request)
        {
            Request = request;
            Response = new TResponse { CorrelationGuid = request.CorrelationGuid };
            Handle();

            Contract.Assume(Response != null);
            return Response;
        }

        protected abstract void Handle();

        protected TNewRequest CreateRequest<TNewRequest>()
            where TNewRequest : BaseRequest, new()
        {
            var result = new TNewRequest { CorrelationGuid = Request.CorrelationGuid, Context = Request.Context };
            return result;
        }

        protected TNotification CreateNotification<TNotification>()
            where TNotification : Notification, new()
        {
            var result = new TNotification { CorrelationGuid = Request.CorrelationGuid, Context = Request.Context };
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
