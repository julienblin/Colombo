using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;
using System.Timers;
using Castle.Core.Logging;
using Colombo.Alerts;
using Colombo.HealthCheck;

namespace Colombo.Wcf
{
    /// <summary>
    /// <see cref="IRequestProcessor"/> implementation that uses WCF to transfer requests. See <see cref="IWcfColomboService"/>.
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

                if (!Logger.IsInfoEnabled) return;

                if (alerters.Length == 0)
                    Logger.Info("No alerters has been registered for the WcfClientRequestProcessor.");
                else
                    Logger.InfoFormat("WcfClientRequestProcessor monitoring with the following alerters: {0}", string.Join(", ", alerters.Select(x => x.GetType().Name)));
            }
        }

        private readonly IWcfColomboServiceFactory serviceFactory;

        public WcfClientRequestProcessor(IWcfColomboServiceFactory serviceFactory)
        {
            if (serviceFactory == null) throw new ArgumentNullException("serviceFactory");
            Contract.EndContractBlock();

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
                var task = Task.Factory.StartNew(g =>
                    {
                        var group = (IGrouping<string, BaseRequest>)g;
                        IWcfColomboService wcfService = null;
                        try
                        {
                            wcfService = serviceFactory.CreateChannel(group.Key);
                            Logger.DebugFormat("Sending {0} request(s) to {1}...", group.Count(), ((IClientChannel)wcfService).RemoteAddress.Uri);
                            var asyncResult = wcfService.BeginProcessAsync(group.ToArray(), null, null);
                            asyncResult.AsyncWaitHandle.WaitOne();
                            return wcfService.EndProcessAsync(asyncResult);
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
                const string message = "An exception occured inside one or several WCF endpoint";
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
                for (var i = 0; i < requestsArray.Length; i++)
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

        private int healthCheckHeartBeatInSeconds;

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

                if (healthCheckHeartBeatInSeconds <= 0) return;

                healthCheckTimer = new Timer(healthCheckHeartBeatInSeconds * 1000) {AutoReset = true};
                healthCheckTimer.Elapsed += HealthCheckTimerElapsed;
                healthCheckTimer.Start();
            }
        }

        private void HealthCheckTimerElapsed(object sender, ElapsedEventArgs e)
        {
            IWcfColomboService currentWcfService = null;

            try
            {
                foreach (var wcfService in serviceFactory.CreateChannelsForAllEndPoints())
                {
                    if(wcfService == null) throw new ColomboException("Internal error: channel should not be null.");

                    currentWcfService = wcfService;
                    try
                    {
                        var hcRequest = new HealthCheckRequest();
                        Logger.DebugFormat("Sending healthcheck request to {0}...", ((IClientChannel)currentWcfService).RemoteAddress.Uri);
                        var asyncResult = currentWcfService.BeginProcessAsync(new BaseRequest[] { hcRequest }, null, null);
                        asyncResult.AsyncWaitHandle.WaitOne();
                        currentWcfService.EndProcessAsync(asyncResult);
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
