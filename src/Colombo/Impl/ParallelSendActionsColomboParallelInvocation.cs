using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Core.Logging;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace Colombo.Impl
{
    public class ParallelSendActionsColomboParallelInvocation : BaseColomboParallelInvocation
    {
        private ILogger logger = NullLogger.Instance;
        public ILogger Logger
        {
            get { return logger; }
            set { logger = value; }
        }

        private readonly IMessageProcessor[] messageProcessors;

        public ParallelSendActionsColomboParallelInvocation(IMessageProcessor[] messageProcessors)
        {
            this.messageProcessors = messageProcessors;
        }

        public override void Proceed()
        {
            Logger.Debug("Selecting appropriate processors for all the requests...");

            var requestProcessorMapping = new Dictionary<IMessageProcessor, List<BaseRequest>>();
            foreach (var request in Requests)
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
            Responses = new Response[Requests.Length];

            for (int i = 0; i < Requests.Length; i++)
            {
                Response response = parallelSendActions.Where(x => x.GetResponseFor(Requests[i]) != null).First().GetResponseFor(Requests[i]);
                Responses[i] = response;
            }
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
