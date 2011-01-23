using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using Castle.Core.Logging;

namespace Colombo.Impl.Alerters
{
    /// <summary>
    /// <see cref="IColomboAlerter"/> that writes <see cref="IColomboAlert"/> to the Application event log as Warnings.
    /// </summary>
    public class EventLogColomboAlerter : IColomboAlerter
    {
        /// <summary>
        /// Name of the source in the event log.
        /// </summary>
        public const string SourceName = @"Colombo";

        private ILogger logger = NullLogger.Instance;
        /// <summary>
        /// Logger.
        /// </summary>
        public ILogger Logger
        {
            get { return logger; }
            set { logger = value; }
        }

        private EventLog applicationEventLog;
        private EventLog ApplicationEventLog
        {
            get
            {
                if (applicationEventLog == null)
                {
                    if (!EventLog.SourceExists(SourceName))
                    {
                        EventLog.CreateEventSource(SourceName, "Application");
                    }
                    applicationEventLog = new EventLog {Source = SourceName};
                }
                return applicationEventLog;
            }
        }

        /// <summary>
        /// Called when an alert has been raised.
        /// </summary>
        public void Alert(IColomboAlert alert)
        {
            if (alert == null) throw new ArgumentNullException("alert");
            Contract.EndContractBlock();

            try
            {
                ApplicationEventLog.WriteEntry(alert.ToString(), EventLogEntryType.Warning);
            }
            catch (Exception ex)
            {
                Logger.WarnFormat(ex, "Error while writing alert {0} to Application event log.", alert);
            }
        }
    }
}
