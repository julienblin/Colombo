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
