﻿using System;
using System.Diagnostics.Contracts;
using Castle.Core.Logging;

namespace Colombo.Impl.RequestHandle
{
    /// <summary>
    /// An <see cref="IColomboRequestHandleInvocation"/> that can invoke <see cref="IRequestHandler"/>.
    /// </summary>
    public class RequestHandlerHandleInvocation : BaseRequestHandleInvocation
    {
        private ILogger logger = NullLogger.Instance;
        public ILogger Logger
        {
            get { return logger; }
            set { logger = value; }
        }

        private readonly IRequestHandlerFactory requestHandlerFactory;

        public RequestHandlerHandleInvocation(IRequestHandlerFactory requestHandlerFactory)
        {
            this.requestHandlerFactory = requestHandlerFactory;
        }

        public override void Proceed()
        {
            if (Request == null)
                throw new ColomboException("Internal error: The Request should not be null");

            var requestHandler = requestHandlerFactory.CreateRequestHandlerFor(Request);
            if (requestHandler == null)
                throw new ColomboException(string.Format("Internal error: The request factory return null for {0}", Request));

            Logger.DebugFormat("Handling {0} with {1}...", Request, requestHandler);
            try
            {
                Contract.Assume(Request != null);
                Response = requestHandler.Handle(Request);
            }
            catch (Exception ex)
            {
                Logger.ErrorFormat(ex, "An exception occurred inside {0}.", requestHandler);
                throw;
            }
            finally
            {
                requestHandlerFactory.DisposeRequestHandler(requestHandler);
            }
        }
    }
}
