using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Colombo
{
    [ContractClass(typeof(Contracts.RequestHandlerFactoryContract))]
    public interface IRequestHandlerFactory
    {
        bool CanCreateRequestHandlerFor(BaseRequest request);
        IRequestHandler CreateRequestHandlerFor(BaseRequest request);
        void DisposeRequestHandler(IRequestHandler requestHandler);
    }
}
