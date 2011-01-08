using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Core.Logging;
using System.Diagnostics.Contracts;
using System.ServiceModel.Configuration;
using System.Configuration;
using System.Threading.Tasks;
using System.ServiceModel;
using System.Timers;
using System.Collections.Concurrent;
using Colombo.HealthCheck;
using Colombo.Alerts;

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

        private IColomboAlerter[] alerters = new IColomboAlerter[0];
        public IColomboAlerter[] Alerters
        {
            get { return alerters; }
            set
            {
                if (value == null) throw new ArgumentNullException("Alerters");
                Contract.EndContractBlock();

                alerters = value;

                if (Logger.IsInfoEnabled)
                {
                    if (alerters.Length == 0)
                        Logger.Info("No alerters has been registered for the WcfClientRequestProcessor.");
                    else
                        Logger.InfoFormat("WcfClientRequestProcessor monitoring with the following alerters: {0}", string.Join(", ", alerters.Select(x => x.GetType().Name)));
                }
            }
        }

        private readonly IWcfServiceFactory serviceFactory;

        public WcfClientRequestProcessor(IWcfServiceFactory serviceFactory)
        {
            this.serviceFactory = serviceFactory;
        }

        public bool CanProcess(BaseRequest request)
        {
            if (request == null) throw new ArgumentNullException("request");
            Contract.EndContractBlock();

            var groupName = request.GetGroupName();
            if (string.IsNullOrEmpty(groupName))
                throw new ColomboException(string.Format("Groupname cannot be null or empty for {0}", request));

            return serviceFactory.CanCreateChannelForRequestGroup(groupName);
        }

        public ResponsesGroup Process(IList<BaseRequest> requests)
        {
            if (requests == null) throw new ArgumentNullException("requests");
            Contract.EndContractBlock();

            var requestsGroups = requests.GroupBy(x => x.GetGroupName());
            foreach (var requestsGroup in requestsGroups)
            {
                Contract.Assume(requestsGroup.Key != null);
                if (!serviceFactory.CanCreateChannelForRequestGroup(requestsGroup.Key))
                    throw new ColomboException(string.Format("Internal error: Unable to send to {0}.", requestsGroup.Key));
            }

            if (Logger.IsDebugEnabled)
            {
                Logger.Debug("Mapping for requests/uri is the following:");
                foreach (var requestsGroup in requestsGroups)
                {
                    Contract.Assume(requestsGroup.Key != null);
                    Logger.DebugFormat("{0} => {{", serviceFactory.GetAddressForRequestGroup(requestsGroup.Key));
                    foreach (var request in requestsGroup)
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
                        IWcfService wcfService = null;
                        try
                        {
                            wcfService = serviceFactory.CreateChannel(group.Key);
                            Logger.DebugFormat("Sending {0} request(s) to {1}...", group.Count(), ((IClientChannel)wcfService).RemoteAddress.Uri);
                            return wcfService.Process(group.ToArray());
                        }
                        finally
                        {
                            if (wcfService != null)
                            {
                                try
                                {
                                    ((IClientChannel)wcfService).Close();
                                }
                                catch (Exception)
                                {
                                    ((IClientChannel)wcfService).Abort();
                                }
                            }
                        }
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
                foreach (var innerEx in ex.InnerExceptions)
                    Logger.Error(innerEx.ToString());
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
        }

        private Timer healthCheckTimer;

        private int healthCheckHeartBeatInSeconds = 0;

        public int HealthCheckHeartBeatInSeconds
        {
            get { return healthCheckHeartBeatInSeconds; }
            set
            {
                healthCheckHeartBeatInSeconds = value;
                if (healthCheckTimer != null)
                {
                    healthCheckTimer.Stop();
                    healthCheckTimer = null;
                }

                if (healthCheckHeartBeatInSeconds > 0)
                {
                    healthCheckTimer = new Timer(healthCheckHeartBeatInSeconds * 1000);
                    healthCheckTimer.AutoReset = true;
                    healthCheckTimer.Elapsed += HealthCheckTimerElapsed;
                    healthCheckTimer.Start();
                }
            }
        }

        private void HealthCheckTimerElapsed(object sender, ElapsedEventArgs e)
        {
            IWcfService currentWcfService = null;

            try
            {
                foreach (var wcfService in serviceFactory.CreateChannelsForAllEndPoints())
                {
                    currentWcfService = wcfService;
                    try
                    {
                        var hcRequest = new HealthCheckRequest();
                        Logger.DebugFormat("Sending healthcheck request to {0}...", ((IClientChannel)currentWcfService).RemoteAddress.Uri);
                        currentWcfService.Process(new BaseRequest[] { hcRequest });
                        Logger.DebugFormat("Healthcheck OK for {0}...", ((IClientChannel)currentWcfService).RemoteAddress.Uri);
                    }
                    catch (Exception ex)
                    {
                        var alert = new HealthCheckFailedAlert(Environment.MachineName, ((IClientChannel)currentWcfService).RemoteAddress.Uri.ToString(), ex);
                        Logger.Warn(alert.ToString());
                        foreach (var alerter in Alerters)
                        {
                            alerter.Alert(alert);
                        }

                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("An error occured while executing a health check.", ex);
            }
            finally
            {
                if (currentWcfService != null)
                {
                    try
                    {
                        ((IClientChannel)currentWcfService).Close();
                    }
                    catch (Exception)
                    {
                        ((IClientChannel)currentWcfService).Abort();
                    }
                }
            }
        }
    }
}
