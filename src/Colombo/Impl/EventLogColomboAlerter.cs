using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Castle.Core.Logging;
using System.Diagnostics.Contracts;

namespace Colombo.Impl
{
    public class EventLogColomboAlerter : IColomboAlerter
    {
        public const string SourceName = @"Colombo";

        private ILogger logger = NullLogger.Instance;
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
                    applicationEventLog = new EventLog();
                    applicationEventLog.Source = SourceName;
                }
                return applicationEventLog;
            }
        }

        public EventLogColomboAlerter()
        {

        }

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
