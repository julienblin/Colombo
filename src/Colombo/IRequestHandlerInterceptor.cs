using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Colombo
{
    [ContractClass(typeof(Contracts.RequestHandlerInterceptorContract))]
    public interface IRequestHandlerInterceptor : IInterceptor
    {
        Response BeforeHandle(BaseRequest request);
        void AfterHandle(BaseRequest request, Response response);
    }
}
