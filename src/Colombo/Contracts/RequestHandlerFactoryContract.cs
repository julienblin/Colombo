using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Colombo.Contracts
{
    [ContractClassFor(typeof(IRequestHandlerFactory))]
    public abstract class RequestHandlerFactoryContract : IRequestHandlerFactory
    {
        bool IRequestHandlerFactory.CanCreateRequestHandlerFor(BaseRequest request)
        {
            Contract.Requires<ArgumentNullException>(request != null, "request");
            return default(bool);
        }

        IRequestHandler IRequestHandlerFactory.CreateRequestHandlerFor(BaseRequest request)
        {
            Contract.Requires<ArgumentNullException>(request != null, "request");
            return default(IRequestHandler);
        }

        void IRequestHandlerFactory.DisposeRequestHandler(IRequestHandler requestHandler)
        {
            Contract.Requires<ArgumentNullException>(requestHandler != null, "requestHandler");
        }
    }
}
