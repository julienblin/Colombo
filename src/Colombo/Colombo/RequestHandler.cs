using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private TRequest request;
        protected TRequest Request { get { return request; } }

        private TResponse response;
        protected TResponse Response { get { return response; } }

        public virtual Response Handle(BaseRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();

            return Handle((TRequest)request);
        }

        public virtual TResponse Handle(TRequest request)
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
