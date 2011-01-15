using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using Castle.Core.Logging;
using Colombo.Alerts;

namespace Colombo.Interceptors
{
    public class SLASendInterceptor : IMessageBusSendInterceptor
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
                    Logger.Info("No alerters has been registered for SLA monitoring.");
                else
                    Logger.InfoFormat("SLA monitoring with the following alerters: {0}", string.Join(", ", alerters.Select(x => x.GetType().Name)));
            }
        }

        public void Intercept(IColomboSendInvocation invocation)
        {
            if (invocation == null) throw new ArgumentNullException("invocation");
            Contract.EndContractBlock();

            var watch = new Stopwatch();
            watch.Start();
            invocation.Proceed();
            watch.Stop();
            Logger.DebugFormat("{0} requests took {1} ms.", invocation.Requests.Count, watch.ElapsedMilliseconds);

            var maxSla = TimeSpan.MinValue;

            foreach (var request in invocation.Requests)
            {
                var slaAttribute = request.GetCustomAttribute<SLAAttribute>();
                if (slaAttribute == null) continue;

                if (maxSla < slaAttribute.Allowed)
                    maxSla = slaAttribute.Allowed;
            }

            if (maxSla != TimeSpan.MinValue)
            {
                if (watch.Elapsed <= maxSla) return;

                var alert = new SLABreachedAlert(invocation.Requests.ToArray(), maxSla, watch.Elapsed);
                Logger.Warn(alert.ToString());
                foreach (var alerter in Alerters)
                {
                    alerter.Alert(alert);
                }
            }
        }

        public int InterceptionPriority
        {
            get { return InterceptorPrority.ReservedHigh; }
        }
    }
}
