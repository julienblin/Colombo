using System;
using System.Diagnostics;
using Colombo.Impl.Alerters;
using NUnit.Framework;

namespace Colombo.Tests.Impl.Alerters
{
    [TestFixture]
    public class EventLogColomboAlerterTest
    {
        [Test]
        public void It_should_write_Alert_ToString_content_to_Application_event_log()
        {
            var alert = new TestAlert();
            var alerter = new EventLogColomboAlerter();

            alerter.Alert(alert);

            var applicationEventLog = new EventLog { Source = EventLogColomboAlerter.SourceName };
            var containsEntry = false;
            foreach (EventLogEntry entry in applicationEventLog.Entries)
            {
                if (entry.Message.Equals(alert.ToString(), StringComparison.InvariantCultureIgnoreCase))
                    containsEntry = true;
            }
            Assert.That(containsEntry);
        }

        public class TestAlert : IColomboAlert
        {
            public override string ToString()
            {
                return "AlertContent";
            }
        }
    }
}
