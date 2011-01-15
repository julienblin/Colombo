using System;
using System.Diagnostics.Contracts;

namespace Colombo.Contracts
{
#pragma warning disable 1591 // docs
    [ContractClassFor(typeof(IRequestHandler<,>))]
    public abstract class GenericRequestHandlerContract<TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
        where TResponse : Response, new()
        where TRequest : Request<TResponse>, new()
    {
        public TResponse Handle(TRequest request)
        {
            Contract.Requires<ArgumentNullException>(request != null, "request");
            Contract.Ensures(Contract.Result<TResponse>() != null);
            throw new NotImplementedException();
        }

        public Response Handle(BaseRequest request)
        {
            throw new NotImplementedException();
        }

        public Type GetRequestType()
        {
            Contract.Ensures(Contract.Result<Type>() != null);
            throw new NotImplementedException();
        }

        public Type GetResponseType()
        {
            Contract.Ensures(Contract.Result<Type>() != null);
            throw new NotImplementedException();
        }
    }
#pragma warning restore 1591
}
