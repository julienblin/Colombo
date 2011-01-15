using System;
using System.Diagnostics.Contracts;
using System.Linq;
using Castle.Core.Logging;
using Castle.MicroKernel;

namespace Colombo.Impl.RequestHandle
{
    /// <summary>
    /// Implementation of <see cref="IRequestHandlerFactory"/> that uses <see cref="Castle.MicroKernel.IKernel"/>.
    /// </summary>
    public class KernelRequestHandlerFactory : IRequestHandlerFactory
    {
        private readonly IKernel kernel;

        public KernelRequestHandlerFactory(IKernel kernel)
        {
            if (kernel == null) throw new ArgumentNullException("kernel");
            Contract.EndContractBlock();

            this.kernel = kernel;
        }

        public bool CanCreateRequestHandlerFor(BaseRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();

            var requestHandlerType = CreateRequestHandlerTypeFrom(request);
            return kernel.HasComponent(requestHandlerType);
        }

        public IRequestHandler CreateRequestHandlerFor(BaseRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();

            var requestHandlerType = CreateRequestHandlerTypeFrom(request);
            var allRequestHandlers = (IRequestHandler[])kernel.ResolveAll(requestHandlerType);

            Contract.Assume(allRequestHandlers != null);
            var chosenRequestsHandlers = allRequestHandlers
                .Where(h =>
                {
                    var chooseAttr = h.GetCustomAttribute<ChooseWhenRequestContextContainsAttribute>();
                    return ((chooseAttr == null) || (chooseAttr.IsChoosen(request)));
                }).ToArray();


            if (chosenRequestsHandlers.Length == 1)
                return chosenRequestsHandlers[0];

            if (chosenRequestsHandlers.Length > 1)
            {
                var specializedRequestsHandler = chosenRequestsHandlers
                    .Where(h => h.GetCustomAttribute<ChooseWhenRequestContextContainsAttribute>() != null)
                    .ToArray();

                if (specializedRequestsHandler.Length == 1)
                    return specializedRequestsHandler[0];
                
                throw new ColomboException(string.Format("Too many request handlers to choose from for {0}: {1}",
                    request,
                    string.Join(", ", chosenRequestsHandlers.Select(x => x.ToString()))));
            }

            throw new ColomboException(string.Format("Request Handler {0} not found for {1}.", requestHandlerType, request));
        }

        public void DisposeRequestHandler(IRequestHandler requestHandler)
        {
            if (requestHandler == null) throw new ArgumentNullException("requestHandler");
            Contract.EndContractBlock();

            kernel.ReleaseComponent(requestHandler);
        }

        private static Type CreateRequestHandlerTypeFrom(BaseRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();

            var responseType = request.GetResponseType();
            var requestType = request.GetType();

            Contract.Assume(typeof(Request<>).IsGenericTypeDefinition);
            Contract.Assume(typeof(Request<>).GetGenericArguments().Length == 1);
            Contract.Assume(typeof(IRequestHandler<,>).IsGenericTypeDefinition);
            Contract.Assume(typeof(IRequestHandler<,>).GetGenericArguments().Length == 2);
            var stdRequestType = typeof(Request<>).MakeGenericType(request.GetResponseType());
            if (stdRequestType.IsAssignableFrom(requestType))
                return typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);

            Contract.Assume(typeof(SideEffectFreeRequest<>).IsGenericTypeDefinition);
            Contract.Assume(typeof(SideEffectFreeRequest<>).GetGenericArguments().Length == 1);
            Contract.Assume(typeof(ISideEffectFreeRequestHandler<,>).IsGenericTypeDefinition);
            Contract.Assume(typeof(ISideEffectFreeRequestHandler<,>).GetGenericArguments().Length == 2);
            var sideEffectFreeRequestType = typeof(SideEffectFreeRequest<>).MakeGenericType(request.GetResponseType());
            if (sideEffectFreeRequestType.IsAssignableFrom(requestType))
                return typeof(ISideEffectFreeRequestHandler<,>).MakeGenericType(requestType, responseType);

            throw new ColomboException(string.Format("Internal error: unable to create request handler generic type for: {0}", request));
        }
    }
}
