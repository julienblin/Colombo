using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Castle.Core.Logging;
using System.Threading.Tasks;

namespace Colombo.Impl
{
    /// <summary>
    /// A <see cref="IColomboSendInvocation"/> that can invoke <see cref="IRequestProcessor"/>.
    /// </summary>
    public class RequestProcessorSendInvocation : BaseSendInvocation
    {
        private readonly IRequestProcessor[] requestProcessors;

        private ILogger logger = NullLogger.Instance;
        public ILogger Logger
        {
            get { return logger; }
            set { logger = value; }
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="requestProcessors">List of <see cref="IRequestProcessor"/> that could process the request.</param>
        public RequestProcessorSendInvocation(IRequestProcessor[] requestProcessors)
        {
            if ((requestProcessors == null) || (requestProcessors.Length == 0)) throw new ArgumentException("requestProcessors should have at least one IRequestProcessor.");
            Contract.EndContractBlock();

            this.requestProcessors = requestProcessors;
        }

        public override void Proceed()
        {
            Logger.Debug("Selecting appropriate processors for all the requests...");

            var requestProcessorMapping = new Dictionary<IRequestProcessor, List<BaseRequest>>();
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

            Logger.Debug("Parallel processing of the requests with the selected processors...");
            var tasks = new List<Task<IDictionary<BaseRequest, Response>>>();
            var tasksProcessorAssociation = new Dictionary<IRequestProcessor, Task<IDictionary<BaseRequest, Response>>>();
            foreach (var processor in requestProcessorMapping.Keys)
            {
                var task = Task.Factory.StartNew<IDictionary<BaseRequest, Response>>((proc) => 
                    {
                        return ((IRequestProcessor)proc).Process(requestProcessorMapping[(IRequestProcessor)proc].ToArray());
                    },
                    processor
                );
                tasks.Add(task);
                tasksProcessorAssociation[processor] = task;
            }
            Task.WaitAll(tasks.ToArray());

            if (tasks.Any(t => t.IsFaulted))
            {
                var faultedTasks = tasks.Where(t => t.IsFaulted);
                var message = string.Format("The following exceptions where raised during the treatment of the requests: {0}", string.Join(", ", faultedTasks.Select(t => t.Exception.ToString())));
                Logger.Error(message);
                throw new ComposedColomboException(message, faultedTasks.Select(t => t.Exception).ToArray());
            }
            else
            {
                Logger.Debug("All the processors have executed successfully.");
            }

            Logger.Debug("Reconstituing responses...");
            Responses = new ResponsesGroup();

            foreach (var request in Requests)
            {
                var processorAndRequestsThatProcessedTheRequest = requestProcessorMapping.Where((pair) => pair.Value.Contains(request)).First();
                var taskThatExecutedTheRequest = tasksProcessorAssociation[processorAndRequestsThatProcessedTheRequest.Key];
                Responses[request] = taskThatExecutedTheRequest.Result[request];
            }
        }

        private IRequestProcessor SelectAppropriateProcessorFor(BaseRequest request)
        {
            Contract.Assume(requestProcessors != null);
            var selectedProcessors = requestProcessors.Where(x => (x != null) && (x.CanSend(request))).ToArray();

            if (selectedProcessors.Length == 0)
                LogAndThrowError("Unable to select an appropriate IRequestProcessor for {0} in {1}.", request, string.Join(", ", requestProcessors.Select(x => x.ToString())));

            if (selectedProcessors.Length > 1)
                LogAndThrowError("Too many IRequestProcessor for {0} in {1}.", request, string.Join(", ", requestProcessors.Select(x => x.ToString())));

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
