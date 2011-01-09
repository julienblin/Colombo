using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    }
}
