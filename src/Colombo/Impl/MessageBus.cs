using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Core.Logging;
using System.Diagnostics.Contracts;

namespace Colombo.Impl
{
    public class MessageBus : IMessageBus
    {
        private ILogger logger = NullLogger.Instance;
        public ILogger Logger
        {
            get { return logger; }
            set { logger = value; }
        }

        private IMessageBusSendInterceptor[] messageBusSendInterceptors = new IMessageBusSendInterceptor[0];
        public IMessageBusSendInterceptor[] MessageBusSendInterceptors
        {
            get { return messageBusSendInterceptors; }
            set
            {
                if (value == null) throw new ArgumentNullException("MessageBusSendInterceptors");
                Contract.EndContractBlock();

                messageBusSendInterceptors = value.OrderBy(x => x.InterceptionPriority).ToArray();
            }
        }

        private readonly IMessageProcessor[] messageProcessors;

        public MessageBus(IMessageProcessor[] messageProcessors)
        {
            if ((messageProcessors == null) || (messageProcessors.Length == 0)) throw new ArgumentException("messageProcessors should have at least one IMessageProcessor.");
            Contract.EndContractBlock();
            
            this.messageProcessors = messageProcessors;
        }

        public virtual TResponse Send<TResponse>(Request<TResponse> request)
            where TResponse : Response, new()
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();
            Contract.Assume(messageProcessors != null);
            Contract.Assume(MessageBusSendInterceptors != null);

            Logger.DebugFormat("Sending request {0}...", request);
            Logger.DebugFormat("Selecting appropriate processor for request {0}...", request);

            var selectedProcessors = messageProcessors.Where(x => (x != null) && (x.CanSend(request)));
            var numSelectedProcessors = selectedProcessors.Count();

            if (numSelectedProcessors == 0)
                LogAndThrowError("Unable to select an appropriate IMessageProcessor for request {0} in {1}.", request, string.Join(", ", messageProcessors.Select(x => x.ToString())));

            if (numSelectedProcessors > 1)
                LogAndThrowError("Too many IMessageProcessor for request {0} in {1}.", request, string.Join(", ", messageProcessors.Select(x => x.ToString())));

            var messageProcessor = messageProcessors.First();
            Logger.DebugFormat("Using IMessageProcessor {0} to send request {1}.", messageProcessor, request);

            Response response = null;

            Logger.DebugFormat("Performing BeforeSend on the {0} registered interceptor(s).", MessageBusSendInterceptors.Length);
            foreach (var sendInterceptor in MessageBusSendInterceptors)
            {
                Logger.DebugFormat("Calling BeforeSend for interceptor {0} and request {1}...", sendInterceptor, request);
                response = sendInterceptor.BeforeSend<TResponse>(request);
                if (response != null)
                {
                    Logger.DebugFormat("Interceptor {0} has responded in BeforeSend - Request {1} will not be sent.", sendInterceptor, request);
                    break;
                }
            }

            if (response == null)
            {
                response = messageProcessor.Send(request);
            }
            Contract.Assert(response != null);

            Logger.DebugFormat("Performing AfterMessageProcessorSend on the {0} registered interceptor(s).", MessageBusSendInterceptors.Length);
            Contract.Assume(MessageBusSendInterceptors != null);
            foreach (var sendInterceptor in MessageBusSendInterceptors.Reverse())
            {
                Logger.DebugFormat("Calling AfterMessageProcessorSend for interceptor {0} and request {1}...", sendInterceptor, request);
                sendInterceptor.AfterMessageProcessorSend(request, response);
            }

            var typedResponse = response as TResponse;
            if (typedResponse == null)
                LogAndThrowError("Received a response of type {0}, but expected {1}.", response.GetType(), typeof(TResponse));

            return typedResponse;
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
