using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Core.Logging;
using System.Diagnostics.Contracts;
using System.ServiceModel.Configuration;
using System.Configuration;
using System.Threading.Tasks;

namespace Colombo.Wcf
{
    /// <summary>
    /// <see cref="IRequestProcessor"/> implementation that uses WCF to transfer requests. See <see cref="IWcfService"/>.
    /// </summary>
    public class WcfClientRequestProcessor : IRequestProcessor
    {
        private ILogger logger = NullLogger.Instance;
        public ILogger Logger
        {
            get { return logger; }
            set { logger = value; }
        }

        public bool CanProcess(BaseRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();

            var channelEndpoint = GetChannelEndpointElement(request.GetGroupName());
            return channelEndpoint != null;
        }

        public ResponsesGroup Process(IList<BaseRequest> requests)
        {
            if (requests == null) throw new ArgumentNullException("requests");
            Contract.EndContractBlock();

            var requestsGroups = requests.GroupBy(x => x.GetGroupName());
            foreach (var requestsGroup in requestsGroups)
            {
                if (GetChannelEndpointElement(requestsGroup.Key) == null)
                    throw new ColomboException(string.Format("Internal error: Unable to send to {0}.", requestsGroup.Key));
            }

            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("Mapping for requests/uri is the following:");
                foreach (var requestGroup in requestsGroups)
                {
                    Logger.DebugFormat("{0} => {{", GetChannelEndpointElement(requestGroup.Key).Address.AbsoluteUri);
                    foreach (var request in requestGroup)
                    {
                        Logger.DebugFormat("  {0}", request);
                    }
                    Logger.Debug("}");
                }
            }

            var tasks = new List<Task<Response[]>>();
            var tasksGroupAssociation = new Dictionary<string, Task<Response[]>>();
            foreach (var requestGroup in requestsGroups)
            {
                var task = Task.Factory.StartNew<Response[]>((g) =>
                    {
                        var group = (IGrouping<string, BaseRequest>)g;
                        var clientBase = CreateClientBase(group.Key);

                        Logger.DebugFormat("Sending {0} request(s) to {1}...", group.Count(), clientBase.Endpoint.Address.Uri);
                        return clientBase.Process(group.ToArray());
                    },
                    requestGroup
                );
                tasks.Add(task);
                tasksGroupAssociation[requestGroup.Key] = task;
            }
            try
            {
                Task.WaitAll(tasks.ToArray());
            }
            catch (AggregateException ex)
            {
                string message = "An exception occured inside one or several WCF endpoint";
                Logger.Error(message, ex);
                throw new ColomboException(message, ex);
            }

            Logger.Debug("All the WCF clients have executed successfully.");

            Logger.Debug("Reconstituing responses...");
            var responses = new ResponsesGroup();
            foreach (var requestsGroup in requestsGroups)
            {
                var task = tasksGroupAssociation[requestsGroup.Key];
                var requestsArray = requestsGroup.ToArray();
                for (int i = 0; i < requestsArray.Length; i++)
                {
                    var request = requestsArray[i];
                    responses[request] = task.Result[i];
                }
            }

            Logger.DebugFormat("{0} responses are returned.", responses.Count);

            Contract.Assume(responses.Count == requests.Count);
            return responses;

            throw new NotImplementedException();
        }

        public WcfClientBaseService CreateClientBase(string groupName)
        {
            Contract.Assume(groupName != null);
            try
            {
                return new WcfClientBaseService(groupName);
            }
            catch (InvalidOperationException ex)
            {
                var errorMessage = string.Format("Unable to create a WCF ClientBase. Did you create a WCF client endPoint with the name {0}?", groupName);
                Logger.Error(errorMessage, ex);
                throw new ColomboException(errorMessage, ex);
            }
        }

        public ChannelEndpointElement GetChannelEndpointElement(string endPointName)
        {
            if (WcfConfigClientSection == null)
                return null;

            foreach (ChannelEndpointElement endPoint in WcfConfigClientSection.Endpoints)
            {
                if (endPoint.Name.Equals(endPointName, StringComparison.InvariantCultureIgnoreCase))
                    return endPoint;
            }

            return null;
        }

        private ClientSection clientSection = null;

        public ClientSection WcfConfigClientSection
        {
            get
            {
                if (clientSection == null)
                {
                    Configuration configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    ServiceModelSectionGroup serviceModelGroup = ServiceModelSectionGroup.GetSectionGroup(configuration);
                    if (serviceModelGroup != null)
                        clientSection = serviceModelGroup.Client;
                }
                return clientSection;
            }
        }
    }
}
