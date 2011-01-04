using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Core.Logging;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

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
                if (Logger.IsInfoEnabled)
                {
                    if(messageBusSendInterceptors.Length == 0)
                        Logger.Info("No interceptor has been registered for sending.");
                    else
                        Logger.InfoFormat("Sending with the following interceptors: {0}", string.Join(", ", messageBusSendInterceptors.Select(x => x.GetType().Name)));
                }
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

            Logger.DebugFormat("Sending request {0}...", request);
            Logger.DebugFormat("Selecting appropriate processor for request {0}...", request);

            var messageProcessor = SelectAppropriateProcessorFor(request);
            Logger.DebugFormat("Using {0} to send request {1}.", messageProcessor, request);

            IColomboInvocation topInvocation = BuildSendInvocationChain(request, messageProcessor);
            topInvocation.Proceed();

            if (topInvocation.Response == null)
                LogAndThrowError("Internal error: received a null response for request {0}", request);

            var typedResponse = topInvocation.Response as TResponse;
            if (typedResponse == null)
                LogAndThrowError("Received a response of type {0}, but expected {1}.", topInvocation.Response.GetType(), typeof(TResponse));

            Logger.DebugFormat("Received {0} initiated by {1}.", typedResponse, request);
            return typedResponse;
        }

        public ResponseGroup<TFirstResponse, TSecondResponse> ParallelSend<TFirstResponse, TSecondResponse>
            (Request<TFirstResponse> firstRequest, Request<TSecondResponse> secondRequest)
            where TFirstResponse : Response, new()
            where TSecondResponse : Response, new()
        {
            if (firstRequest == null) throw new ArgumentNullException("firstRequest");
            if (secondRequest == null) throw new ArgumentNullException("secondRequest");
            Contract.EndContractBlock();

            var responses = InternalParallelSend(firstRequest, secondRequest);
            Contract.Assume(responses != null);
            Contract.Assume(responses.Length == 2);
            return new ResponseGroup<TFirstResponse, TSecondResponse>(responses);
        }

        public ResponseGroup<TFirstResponse, TSecondResponse, TThirdResponse> ParallelSend<TFirstResponse, TSecondResponse, TThirdResponse>
            (Request<TFirstResponse> firstRequest, Request<TSecondResponse> secondRequest, Request<TThirdResponse> thirdRequest)
            where TFirstResponse : Response, new()
            where TSecondResponse : Response, new()
            where TThirdResponse : Response, new()
        {
            if (firstRequest == null) throw new ArgumentNullException("firstRequest");
            if (secondRequest == null) throw new ArgumentNullException("secondRequest");
            if (thirdRequest == null) throw new ArgumentNullException("thirdRequest");
            Contract.EndContractBlock();

            var responses = InternalParallelSend(firstRequest, secondRequest, thirdRequest);
            Contract.Assume(responses != null);
            Contract.Assume(responses.Length == 3);
            return new ResponseGroup<TFirstResponse, TSecondResponse, TThirdResponse>(responses);
        }

        public ResponseGroup<TFirstResponse, TSecondResponse, TThirdResponse, TFourthResponse> ParallelSend<TFirstResponse, TSecondResponse, TThirdResponse, TFourthResponse>
            (Request<TFirstResponse> firstRequest, Request<TSecondResponse> secondRequest, Request<TThirdResponse> thirdRequest, Request<TFourthResponse> fourthRequest)
            where TFirstResponse : Response, new()
            where TSecondResponse : Response, new()
            where TThirdResponse : Response, new()
            where TFourthResponse : Response, new()
        {
            if (firstRequest == null) throw new ArgumentNullException("firstRequest");
            if (secondRequest == null) throw new ArgumentNullException("secondRequest");
            if (thirdRequest == null) throw new ArgumentNullException("thirdRequest");
            if (fourthRequest == null) throw new ArgumentNullException("fourthRequest");
            Contract.EndContractBlock();

            var responses = InternalParallelSend(firstRequest, secondRequest, thirdRequest, fourthRequest);
            Contract.Assume(responses != null);
            Contract.Assume(responses.Length == 4);
            return new ResponseGroup<TFirstResponse, TSecondResponse, TThirdResponse, TFourthResponse>(responses);
        }

        private Response[] InternalParallelSend(params BaseRequest[] requests)
        {
            if (requests == null) throw new ArgumentNullException("requests");
            Contract.EndContractBlock();
            Contract.Assume(messageProcessors != null);

            if(Logger.IsDebugEnabled)
                Logger.DebugFormat("Sending {0} requests in parallel: {1}...", requests.Length, string.Join(", ", requests.Select(x => x.ToString())));

            Logger.Debug("Selecting appropriate processors for all the requests...");

            var requestProcessorMapping = new Dictionary<IMessageProcessor, List<BaseRequest>>();
            foreach (var request in requests)
            {
                var processor = SelectAppropriateProcessorFor(request);
                if (!requestProcessorMapping.ContainsKey(processor))
                    requestProcessorMapping[processor] = new List<BaseRequest>();
                requestProcessorMapping[processor].Add(request);
            }

            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("Mapping for processors is the following:");
                foreach (var processor in requestProcessorMapping.Keys)
                {
                    Logger.DebugFormat("{0} => {{", processor);
                    foreach (var request in requestProcessorMapping[processor])
                    {
                        Logger.DebugFormat("  {0}", request);
                    }
                    Logger.Debug("}");
                }
            }

            var parallelSendActions = new List<ParallelSendAction>();
            foreach (var processor in requestProcessorMapping.Keys)
            {
                Contract.Assume(processor != null);
                var action = new ParallelSendAction(processor, requestProcessorMapping[processor].ToArray());
                parallelSendActions.Add(action);
            }

            Logger.DebugFormat("Executing {0} parallel actions...", parallelSendActions.Count);
            Parallel.ForEach(parallelSendActions, (action) =>
            {
                action.Execute();
            });

            Logger.Debug("Reconstituing responses...");
            var responses = new Response[requests.Length];

            for (int i = 0; i < requests.Length; i++)
            {
                Response response = parallelSendActions.Where(x => x.GetResponseFor(requests[i]) != null).First().GetResponseFor(requests[i]);
                responses[i] = response;
            }
            
            return responses;
        }

        private IMessageProcessor SelectAppropriateProcessorFor(BaseRequest request)
        {
            Contract.Assume(messageProcessors != null);
            var selectedProcessors = messageProcessors.Where(x => (x != null) && (x.CanSend(request))).ToArray();

            if (selectedProcessors.Length == 0)
                LogAndThrowError("Unable to select an appropriate IMessageProcessor for {0} in {1}.", request, string.Join(", ", messageProcessors.Select(x => x.ToString())));

            if (selectedProcessors.Length > 1)
                LogAndThrowError("Too many IMessageProcessor for {0} in {1}.", request, string.Join(", ", messageProcessors.Select(x => x.ToString())));

            return selectedProcessors[0];
        }

        private IColomboInvocation BuildSendInvocationChain(BaseRequest request, IMessageProcessor messageProcessor)
        {
            Contract.Assume(request != null);
            Contract.Assume(messageProcessor != null);
            Contract.Assume(MessageBusSendInterceptors != null);

            IColomboInvocation currentInvocation = new MessageProcessorSendColomboInvocation(ColomboInvocationType.Send, request, messageProcessor);
            foreach (var interceptor in MessageBusSendInterceptors.Reverse())
            {
                if(interceptor != null)
                    currentInvocation = new InterceptorColomboInvocation(ColomboInvocationType.Send, request, interceptor, currentInvocation);
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
