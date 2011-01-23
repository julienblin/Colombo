using System;
using System.Diagnostics.Contracts;

namespace Colombo.Contracts
{
#pragma warning disable 1591 // docs
    [ContractClassFor(typeof(IRequestHandler))]
    public abstract class RequestHandlerContract : IRequestHandler
    {
        public Response Handle(BaseRequest request)
        {
            Contract.Requires<ArgumentNullException>(request != null, "request");
            Contract.Ensures(Contract.Result<Response>() != null);
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
