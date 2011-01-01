using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Core.Logging;
using System.Diagnostics.Contracts;
using System.Transactions;

namespace Colombo.Impl
{
    public class LocalMessageProcessor : IMessageProcessor
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
            }
        }

        private readonly IRequestHandlerFactory requestHandlerFactory;

        public LocalMessageProcessor(IRequestHandlerFactory requestHandlerFactory)
        {
            if (requestHandlerFactory == null) throw new ArgumentNullException("requestHandlerFactory");
            Contract.EndContractBlock();

            this.requestHandlerFactory = requestHandlerFactory;
        }

        public bool CanSend<TResponse>(Request<TResponse> request)
            where TResponse : Response, new()
        {
            return requestHandlerFactory.CanCreateRequestHandlerFor(request);
        }

        public TResponse Send<TResponse>(Request<TResponse> request)
            where TResponse : Response, new()
        {
            using (var tx = new TransactionScope())
            {
                IRequestHandler requestHandler = requestHandlerFactory.CreateRequestHandlerFor(request);
                if (requestHandler == null)
                    LogAndThrowError("Internal error : requestHandler should not be null for {0}", request);

                Contract.Assume(requestHandler != null);
                Logger.DebugFormat("Request {0} is being handled by {1}...", request, requestHandler);

                TResponse response = null;
                try
                {
                    Logger.DebugFormat("Performing BeforeHandle on the {0} registered interceptor(s).", RequestHandlerInterceptor.Length);
                    foreach (var requestHandlerInterceptor in RequestHandlerInterceptor)
                    {
                        Logger.DebugFormat("Calling BeforeHandle for interceptor {0} and request {1}...", requestHandlerInterceptor, request);
                        response = requestHandlerInterceptor.BeforeHandle(request);
                        if (response != null)
                        {
                            Logger.DebugFormat("Interceptor {0} has responded in BeforeHandle - Request {1} will not be handled by requestHandler.", requestHandlerInterceptor, request);
                            break;
                        }
                    }

                    if (response == null)
                    {
                        response = (TResponse)requestHandler.Handle(request);
                    }
                    Contract.Assert(response != null);

                    Logger.DebugFormat("Performing AfterHandle on the {0} registered interceptor(s).", RequestHandlerInterceptor.Length);
                    foreach (var requestHandlerInterceptor in RequestHandlerInterceptor.Reverse())
                    {
                        Logger.DebugFormat("Calling AfterHandle for interceptor {0} and request {1}...", requestHandlerInterceptor, request);
                        requestHandlerInterceptor.AfterHandle(request, response);
                    }

                    tx.Complete();
                }
                catch (Exception ex)
                {
                    Logger.ErrorFormat(ex, "An exception occurred inside requestHandler {0} or one of the interceptors.", requestHandler);
                    throw;
                }
                finally
                {
                    requestHandlerFactory.DisposeRequestHandler(requestHandler);
                }
                return response;
            }
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
