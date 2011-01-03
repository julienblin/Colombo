using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Colombo
{
    [ContractClass(typeof(Contracts.RequestHandlerContract))]
    public interface IRequestHandler
    {
        Response Handle(BaseRequest request);
    }

    [ContractClass(typeof(Contracts.GenericRequestHandlerContract<,>))]
    public interface IRequestHandler<TRequest, TResponse> : IRequestHandler
        where TResponse : Response, new()
        where TRequest : Request<TResponse>, new()
    {
        TResponse Handle(TRequest request);
    }

    public abstract class RequestHandler<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
        where TResponse : Response, new()
        where TRequest : Request<TResponse>, new()
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
