using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Core.Logging;
using Castle.MicroKernel;
using System.Diagnostics.Contracts;

namespace Colombo.Impl
{
    public class KernelRequestHandlerFactory : IRequestHandlerFactory
    {
        private ILogger logger = NullLogger.Instance;
        public ILogger Logger
        {
            get { return logger; }
            set { logger = value; }
        }

        private readonly IKernel kernel;

        public KernelRequestHandlerFactory(IKernel kernel)
        {
            if (kernel == null) throw new ArgumentNullException("kernel");
            Contract.EndContractBlock();

            this.kernel = kernel;
        }

        public bool CanCreateRequestHandlerFor(BaseRequest request)
        {
            var requestHandlerType = CreateIRequestHandlerTypeFrom(request);
            return kernel.HasComponent(requestHandlerType);
        }

        public IRequestHandler CreateRequestHandlerFor(BaseRequest request)
        {
            var requestHandlerType = CreateIRequestHandlerTypeFrom(request);
            try
            {
                return (IRequestHandler)kernel.Resolve(requestHandlerType);
            }
            catch (ComponentNotFoundException ex)
            {
                Logger.ErrorFormat(ex, "ComponentNotFoundException raised for {0} - Should not be the case!", requestHandlerType);
                return null;
            }
        }

        public void DisposeRequestHandler(IRequestHandler requestHandler)
        {
            kernel.ReleaseComponent(requestHandler);
        }

        private static Type CreateIRequestHandlerTypeFrom(BaseRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();
            Contract.Assume(typeof(IRequestHandler<,>).IsGenericTypeDefinition);

            Type responseType = request.GetResponseType();
            Type requestType = request.GetType();

            var requestHandlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);
            return requestHandlerType;
        }
    }
}
