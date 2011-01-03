using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.Diagnostics;
using Castle.Core.Logging;
using Colombo.Impl;
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

                if (Logger.IsInfoEnabled)
                {
                    if (alerters.Length == 0)
                        Logger.Info("No alerters has been registered for SLA monitoring.");
                    else
                        Logger.InfoFormat("SLA monitoring with the following alerters: {0}", string.Join(", ", alerters.Select(x => x.GetType().Name)));
                }
            }
        }

        public void Intercept(IColomboInvocation invocation)
        {
            if (invocation == null) throw new ArgumentNullException("invocation");
            Contract.EndContractBlock();

            var watch = new Stopwatch();
            watch.Start();
            invocation.Proceed();
            watch.Stop();
            Logger.DebugFormat("{0} took {1} ms.", invocation.Request, watch.ElapsedMilliseconds);

            var slaAttribute = invocation.Request.GetCustomAttribute<SLAAttribute>();
            if (slaAttribute != null)
            {
                if (watch.Elapsed > slaAttribute.Allowed)
                {
                    var alert = new SLABreachedAlert(invocation.Request, slaAttribute.Allowed, watch.Elapsed);
                    Logger.Warn(alert.ToString());
                    foreach (var alerter in Alerters)
                    {
                        alerter.Alert(alert);
                    }
                }
            }
        }

        public int InterceptionPriority
        {
            get { return InterceptorPrority.ReservedHigh; }
        }
    }
}
