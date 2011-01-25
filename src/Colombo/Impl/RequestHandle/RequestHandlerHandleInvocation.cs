#region License
// The MIT License
// 
// Copyright (c) 2011 Julien Blin, julien.blin@gmail.com
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
#endregion

using System;
using System.Diagnostics.Contracts;
using Castle.Core.Logging;

namespace Colombo.Impl.RequestHandle
{
    /// <summary>
    /// An <see cref="IColomboRequestHandleInvocation"/> that can invoke <see cref="IRequestHandler"/>.
    /// </summary>
    internal class RequestHandlerHandleInvocation : BaseRequestHandleInvocation
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
