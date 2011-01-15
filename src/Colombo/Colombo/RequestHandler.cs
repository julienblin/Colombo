﻿using System;
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
        protected TRequest Request { get; private set; }

        protected TResponse Response { get; set; }

        public virtual Response Handle(BaseRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();

            return Handle((TRequest)request);
        }

        public virtual TResponse Handle(TRequest request)
        {
            Request = request;
            Response = new TResponse { CorrelationGuid = request.CorrelationGuid };
            Handle();
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
