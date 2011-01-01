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
        TResponse BeforeHandle<TResponse>(Request<TResponse> request) where TResponse : Response, new();
        void AfterHandle<TResponse>(Request<TResponse> request, Response response) where TResponse : Response, new();
    }
}
