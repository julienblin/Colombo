using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Colombo
{
    /// <summary>
    /// Component that can create <see cref="IRequestHandler"/>.
    /// </summary>
    [ContractClass(typeof(Contracts.RequestHandlerFactoryContract))]
    public interface IRequestHandlerFactory
    {
        /// <summary>
        /// <c>true</c> if the factory can create a <see cref="IRequestHandler"/> to handle the request, <c>false</c> otherwise.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        bool CanCreateRequestHandlerFor(BaseRequest request);

        /// <summary>
        /// Creates a <see cref="IRequestHandler"/> to handler the <paramref name="request"/>.
        /// </summary>
        IRequestHandler CreateRequestHandlerFor(BaseRequest request);

        /// <summary>
        /// Dispose the <paramref name="requestHandler"/>.
        /// </summary>
        void DisposeRequestHandler(IRequestHandler requestHandler);
    }
}
