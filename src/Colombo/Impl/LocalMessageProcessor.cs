using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Core.Logging;
using System.Diagnostics.Contracts;
using System.Transactions;

namespace Colombo.Impl
{
    public class LocalMessageProcessor : ILocalMessageProcessor
    {
        private ILogger logger = NullLogger.Instance;
        public ILogger Logger
        {
            get { return logger; }
            set { logger = value; }
        }

        private IRequestHandlerInterceptor[] requestHandlerInterceptor = new IRequestHandlerInterceptor[0];
        public IRequestHandlerInterceptor[] RequestHandlerInterceptor
        {
            get { return requestHandlerInterceptor; }
            set
            {
                if (value == null) throw new ArgumentNullException("RequestHandlerInterceptor");
                Contract.EndContractBlock();

                requestHandlerInterceptor = value.OrderBy(x => x.InterceptionPriority).ToArray();
                Logger.InfoFormat("Using the following interceptors: {0}", string.Join(", ", requestHandlerInterceptor.Select(x => x.GetType().Name)));
            }
        }

        private readonly IRequestHandlerFactory requestHandlerFactory;

        public LocalMessageProcessor(IRequestHandlerFactory requestHandlerFactory)
        {
            if (requestHandlerFactory == null) throw new ArgumentNullException("requestHandlerFactory");
            Contract.EndContractBlock();

            this.requestHandlerFactory = requestHandlerFactory;
        }

        public virtual bool CanSend(BaseRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();

            return requestHandlerFactory.CanCreateRequestHandlerFor(request);
        }

        public virtual Response Send(BaseRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();
            IRequestHandler requestHandler = requestHandlerFactory.CreateRequestHandlerFor(request);
            if (requestHandler == null)
                LogAndThrowError("Internal error : requestHandler should not be null for {0}", request);

            Contract.Assume(requestHandler != null);
            Logger.DebugFormat("{0} is being handled by {1}...", request, requestHandler);

            Response response = null;
            try
            {
                IColomboSingleInvocation topInvocation = BuildInvocationChain(request, requestHandler);
                topInvocation.Proceed();
                response = topInvocation.Response;
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat(ex, "An exception occurred inside {0} or one of the interceptors.", requestHandler);
                throw;
            }
            finally
            {
                requestHandlerFactory.DisposeRequestHandler(requestHandler);
            }

            if(response == null)
                LogAndThrowError("Internal error: received a null response for {0}", request);

            Contract.Assume(response != null);
            return response;
        }

        public Response[] ParallelSend(BaseRequest[] requests)
        {
            if (requests == null) throw new ArgumentNullException("requests");
            Contract.EndContractBlock();

            throw new NotImplementedException();
        }

        private IColomboSingleInvocation BuildInvocationChain(BaseRequest request, IRequestHandler requestHandler)
        {
            Contract.Assume(request != null);
            Contract.Assume(requestHandler != null);
            Contract.Assume(RequestHandlerInterceptor != null);

            IColomboSingleInvocation currentInvocation = new RequestHandlerColomboInvocation(request, requestHandler);
            foreach (var interceptor in RequestHandlerInterceptor.Reverse())
            {
                if (interceptor != null)
                    currentInvocation = new InterceptorColomboSingleInvocation(request, interceptor, currentInvocation);
            }
            return currentInvocation;
        }

        private void LogAndThrowError(string format, params object[] args)
        {
            if (format == null) throw new ArgumentNullException("format");
            if (args == null) throw new ArgumentNullException("args");
            Contract.EndContractBlock();

            var errorMessage = string.Format(format, args);
            Logger.Error(errorMessage);
            throw new ColomboException(errorMessage);
        }
    }
}
