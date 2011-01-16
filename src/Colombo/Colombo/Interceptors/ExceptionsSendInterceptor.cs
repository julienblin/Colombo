using System;
using System.Diagnostics.Contracts;
using System.Linq;
using Castle.Core.Logging;
using Colombo.Alerts;

namespace Colombo.Interceptors
{
    /// <summary>
    /// <see cref="IMessageBusSendInterceptor"/> that sends <see cref="ExceptionInSendAlert"/> when an exception occurs.
    /// </summary>
    public class ExceptionsSendInterceptor : IMessageBusSendInterceptor
    {
        private ILogger logger = NullLogger.Instance;
        /// <summary>
        /// Logger.
        /// </summary>
        public ILogger Logger
        {
            get { return logger; }
            set { logger = value; }
        }

        private IColomboAlerter[] alerters = new IColomboAlerter[0];
        /// <summary>
        /// Alerters to use. All will be notified.
        /// </summary>
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
                    Logger.Info("No alerters has been registered for Exceptions monitoring.");
                else
                    Logger.InfoFormat("Exceptions monitoring with the following alerters: {0}", string.Join(", ", alerters.Select(x => x.GetType().Name)));
            }
        }

        /// <summary>
        /// Alerts when exceptions.
        /// </summary>
        public void Intercept(IColomboSendInvocation nextInvocation)
        {
            try
            {
                nextInvocation.Proceed();
            }
            catch (Exception ex)
            {
                try
                {
                    var alert = new ExceptionInSendAlert(nextInvocation.Requests.ToArray(), ex);
                    Logger.Warn(alert.ToString());
                    foreach (var alerter in Alerters)
                        alerter.Alert(alert);
                }
                catch
                {
                }
                throw;
            }
        }

        /// <summary>
        /// High
        /// </summary>
        public int InterceptionPriority
        {
            get { return InterceptionPrority.High; }
        }
    }
}
