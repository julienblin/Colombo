using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Colombo.Contracts
{
    [ContractClassFor(typeof(IRequestHandler))]
    public abstract class RequestHandlerContract : IRequestHandler
    {
        Response IRequestHandler.Handle(BaseRequest request)
        {
            Contract.Requires<ArgumentNullException>(request != null, "request");
            Contract.Ensures(Contract.Result<Response>() != null);
            return default(Response);
        }
    }
}
