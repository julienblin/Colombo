using System;
using System.Diagnostics.Contracts;

namespace Colombo.Contracts
{
    [ContractClassFor(typeof(IRequestHandlerFactory))]
    public abstract class RequestHandlerFactoryContract : IRequestHandlerFactory
    {
        public bool CanCreateRequestHandlerFor(BaseRequest request)
        {
            Contract.Requires<ArgumentNullException>(request != null, "request");
            throw new NotImplementedException();
        }

        public IRequestHandler CreateRequestHandlerFor(BaseRequest request)
        {
            Contract.Requires<ArgumentNullException>(request != null, "request");
            throw new NotImplementedException();
        }

        public void DisposeRequestHandler(IRequestHandler requestHandler)
        {
            Contract.Requires<ArgumentNullException>(requestHandler != null, "requestHandler");
            throw new NotImplementedException();
        }
    }
}
