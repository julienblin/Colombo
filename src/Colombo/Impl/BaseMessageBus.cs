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
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Security;
using System.Threading.Tasks;
using Castle.Core.Logging;
using Colombo.Impl.Async;

namespace Colombo.Impl
{
    /// <summary>
    /// Base class for <see cref="IMessageBus"/>.
    /// </summary>
    public abstract class BaseMessageBus : IMessageBus, IMetaContextKeysManager
    {
        private ILogger logger = NullLogger.Instance;
        /// <summary>
        /// Logger.
        /// </summary>
        public ILogger Logger
        {
            get { return logger; }
            set { logger = value; }
        }

        private IMessageBusSendInterceptor[] messageBusSendInterceptors = new IMessageBusSendInterceptor[0];
        /// <summary>
        /// The list of <see cref="IMessageBusSendInterceptor"/> to use.
        /// </summary>
        public IMessageBusSendInterceptor[] MessageBusSendInterceptors
        {
            get { return messageBusSendInterceptors; }
            set
            {
                if (value == null) throw new ArgumentNullException("MessageBusSendInterceptors");
                Contract.EndContractBlock();

                messageBusSendInterceptors = value.OrderBy(x => x.InterceptionPriority).ToArray();
                if (!Logger.IsInfoEnabled) return;

                if (messageBusSendInterceptors.Length == 0)
                    Logger.Info("No interceptor has been registered for sending requests.");
                else
                    Logger.InfoFormat("Sending requests with the following interceptors: {0}", string.Join(", ", messageBusSendInterceptors.Select(x => x.GetType().Name)));
            }
        }

        /// <summary>
        /// Disable the management of <see cref="MetaContextKeys"/>.
        /// </summary>
        public bool DoNotManageMetaContextKeys { get; set; }

        /// <summary>
        /// Send synchronously a request and returns the response.
        /// </summary>
        public virtual Response Send(BaseRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();

            var responses = InternalSend(new List<BaseRequest> { request });
            Contract.Assume(responses != null);
            Contract.Assume(responses.Count == 1);

            return responses[request];
        }

        /// <summary>
        /// Send synchronously a request and returns the response.
        /// </summary>
        public virtual TResponse Send<TResponse>(Request<TResponse> request) where TResponse : Response, new()
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();

            var responses = InternalSend(new List<BaseRequest> { request });
            Contract.Assume(responses != null);
            Contract.Assume(responses.Count == 1);

            var typedResponse = responses[request] as TResponse;
            if (typedResponse == null)
                LogAndThrowError("Internal error: InternalSend returned null or incorrect response type: expected {0}, actual {1}.", typeof(TResponse), responses[request].GetType());

            return typedResponse;
        }

        /// <summary>
        /// Send a request asynchronously. You must register a callback with the result to get the response or the error.
        /// </summary>
        public virtual IAsyncCallback<TResponse> SendAsync<TResponse>(Request<TResponse> request)
            where TResponse : Response, new()
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();

            var response = InternalSendAsync<TResponse>(request);
            if (response == null)
                LogAndThrowError("Internal error: response should not be null.");

            Contract.Assume(response != null);
            return response;
        }

        /// <summary>
        /// Send synchronously a request and returns the response.
        /// </summary>
        public virtual TResponse Send<TResponse>(SideEffectFreeRequest<TResponse> request)
            where TResponse : Response, new()
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();

            var responses = InternalSend(new List<BaseRequest> { request });
            Contract.Assume(responses != null);
            Contract.Assume(responses.Count == 1);

            var typedResponse = responses[request] as TResponse;
            if (typedResponse == null)
                LogAndThrowError("Internal error: InternalSend returned null or incorrect response type: expected {0}, actual {1}.", typeof(TResponse), responses[request].GetType());

            return typedResponse;
        }

        /// <summary>
        /// Send a request asynchronously. You must register a callback with the result to get the response or the error.
        /// </summary>
        public virtual IAsyncCallback<TResponse> SendAsync<TResponse>(SideEffectFreeRequest<TResponse> request)
            where TResponse : Response, new()
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();

            var response = InternalSendAsync<TResponse>(request);
            if (response == null)
                LogAndThrowError("Internal error: response should not be null.");

            Contract.Assume(response != null);
            return response;
        }

        /// <summary>
        /// Send synchronously, but in parallel, several requests and returns all the responses at once.
        /// Only side effect-free requests can be parallelized.
        /// </summary>
        public virtual ResponsesGroup Send(BaseSideEffectFreeRequest request, params BaseSideEffectFreeRequest[] followingRequests)
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();

            var listRequests = new List<BaseRequest> { request };
            if (followingRequests != null)
                listRequests.AddRange(followingRequests);
            var responsesGroup = InternalSend(listRequests);

            Contract.Assume(responsesGroup != null);
            return responsesGroup;
        }

        /// <summary>
        /// Number of method in stackframes that will not be captured by CodeOrigin
        /// </summary>
        private const int NumStackFrameToSkipWhenCapturingCodeOrigin = 2;

        /// <summary>
        /// The stacktrace is truncated to avoid big message size.
        /// </summary>
        private const int MaxStrLengthInStackTrace = 255;

        /// <summary>
        /// Real sending of the requests. All the other send methods delegates to this one.
        /// Uses <see cref="BuildSendInvocationChain"/>.
        /// </summary>
        protected virtual ResponsesGroup InternalSend(IList<BaseRequest> requests)
        {
            if (!DoNotManageMetaContextKeys)
            {
                string stackTraceStr = null;
                try
                {
                    if (requests.Any(x => !x.Context.ContainsKey(MetaContextKeys.CodeOrigin)))
                    {
                        var stackTrace = new StackTrace(NumStackFrameToSkipWhenCapturingCodeOrigin, false);
                        stackTraceStr = stackTrace.ToString();
                        if (stackTraceStr.Length > MaxStrLengthInStackTrace)
                            stackTraceStr = stackTraceStr.Substring(0, MaxStrLengthInStackTrace);
                    }
                }
                catch (SecurityException)
                {
                    // Security exception might occured when capturing StackTrace - ignored.
                }

                foreach (var request in requests)
                {
                    request.Context[MetaContextKeys.SenderMachineName] = Environment.MachineName;

                    if (!request.Context.ContainsKey(MetaContextKeys.CodeOrigin) && (stackTraceStr != null))
                        request.Context[MetaContextKeys.CodeOrigin] = stackTraceStr;
                }
            }

            var topInvocation = BuildSendInvocationChain();
            topInvocation.Requests = requests;
            topInvocation.Proceed();

            if (topInvocation.Responses == null)
                throw new ColomboException("Internal error: responses should not be null");

            return topInvocation.Responses;
        }

        /// <summary>
        /// Internal method that is used by Async send operations. Uses <see cref="InternalSend"/> under the cover.
        /// </summary>
        protected virtual IAsyncCallback<TResponse> InternalSendAsync<TResponse>(BaseRequest request)
            where TResponse : Response, new()
        {
            var asyncCallback = new AsyncCallback<TResponse>();
            Task.Factory.StartNew(c =>
            {
                try
                {
                    var responsesGroup = InternalSend(new List<BaseRequest> { request });
                    ((AsyncCallback<TResponse>)c).ResponseArrived((TResponse)responsesGroup[request]);
                }
                catch (Exception ex)
                {
                    ((AsyncCallback<TResponse>)c).ExceptionArrived(ex);
                }
            },
            asyncCallback);

            return asyncCallback;
        }

        /// <summary>
        /// Log an error using the <see cref="Logger"/> and throw a <see cref="ColomboException"/>.
        /// </summary>
        protected virtual void LogAndThrowError(string format, params object[] args)
        {
            if (format == null) throw new ArgumentNullException("format");
            if (args == null) throw new ArgumentNullException("args");
            Contract.EndContractBlock();

            var errorMessage = string.Format(format, args);
            Logger.Error(errorMessage);
            throw new ColomboException(errorMessage);
        }

        /// <summary>
        /// Return a invocation chain for the Send operation.
        /// </summary>
        protected abstract IColomboSendInvocation BuildSendInvocationChain();
    }
}
