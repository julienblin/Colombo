using System;
using System.Diagnostics.Contracts;

namespace Colombo
{
    /// <summary>
    /// Use this base class to create request handlers for standard requests.
    /// </summary>
    public abstract class RequestHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
        where TResponse : Response, new()
        where TRequest : Request<TResponse>, new()
    {
        /// <summary>
        /// Incoming request.
        /// </summary>
        protected TRequest Request { get; private set; }

        /// <summary>
        /// Outgoing response. It will be created before Handle() and the CorrelationGuid will be set.
        /// </summary>
        protected TResponse Response { get; set; }

        /// <summary>
        /// Handles the request.
        /// </summary>
        public virtual Response Handle(BaseRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();

            return Handle((TRequest)request);
        }

        /// <summary>
        /// Handles the request.
        /// </summary>
        public virtual TResponse Handle(TRequest request)
        {
            Request = request;
            Response = new TResponse { CorrelationGuid = request.CorrelationGuid };
            Handle();
            return Response;
        }

        /// <summary>
        /// Handles the request.
        /// </summary>
        protected abstract void Handle();

        /// <summary>
        /// Create a new request to be used inside this request handler.
        /// The CorrelationGuid and the Context are copied.
        /// </summary>
        protected TNewRequest CreateRequest<TNewRequest>()
            where TNewRequest : BaseRequest, new()
        {
            var result = new TNewRequest { CorrelationGuid = Request.CorrelationGuid, Context = Request.Context };
            return result;
        }

        /// <summary>
        /// Create a new notification to be used inside this request handler.
        /// The CorrelationGuid and the Context are copied.
        /// </summary>
        protected TNotification CreateNotification<TNotification>()
            where TNotification : Notification, new()
        {
            var result = new TNotification { CorrelationGuid = Request.CorrelationGuid, Context = Request.Context };
            return result;
        }

        /// <summary>
        /// Get the type of request that this request handler handles.
        /// </summary>
        /// <returns></returns>
        public Type GetRequestType()
        {
            return typeof(TRequest);
        }

        /// <summary>
        /// Get the type of response that this request handler produces.
        /// </summary>
        /// <returns></returns>
        public Type GetResponseType()
        {
            return typeof(TResponse);
        }
    }
}
