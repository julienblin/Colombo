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
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using Castle.Core.Logging;

namespace Colombo.Impl.Send
{
    /// <summary>
    /// A <see cref="IColomboSendInvocation"/> that can invoke <see cref="IRequestProcessor"/>.
    /// </summary>
    internal class RequestProcessorSendInvocation : BaseSendInvocation
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
            var tasks = new List<Task<ResponsesGroup>>();
            var tasksProcessorAssociation = new Dictionary<IRequestProcessor, Task<ResponsesGroup>>();
            foreach (var processor in requestProcessorMapping.Keys)
            {
                var task = Task.Factory.StartNew(proc => ((IRequestProcessor)proc).Process(requestProcessorMapping[(IRequestProcessor)proc].ToArray()),
                    processor
                );
                tasks.Add(task);
                tasksProcessorAssociation[processor] = task;
            }
            try
            {
                Task.WaitAll(tasks.ToArray());
            }
            catch (AggregateException ex)
            {
                const string message = "An exception occured inside one or several processors";
                Logger.Error(message, ex);
                throw new ColomboException(message, ex);
            }
            Logger.Debug("All the processors have executed successfully.");

            Logger.Debug("Reconstituing responses...");
            Responses = new ResponsesGroup();

            foreach (var request in Requests)
            {
                var localRequest = request;
                var processorAndRequestsThatProcessedTheRequest = requestProcessorMapping.Where(pair => pair.Value.Contains(localRequest)).First();
                var taskThatExecutedTheRequest = tasksProcessorAssociation[processorAndRequestsThatProcessedTheRequest.Key];
                Responses[request] = taskThatExecutedTheRequest.Result[request];
            }
        }

        private IRequestProcessor SelectAppropriateProcessorFor(BaseRequest request)
        {
            Contract.Assume(requestProcessors != null);
            var selectedProcessors = requestProcessors.Where(x => (x != null) && (x.CanProcess(request))).ToArray();

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
