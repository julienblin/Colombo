using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Colombo.Interceptors;
using NUnit.Framework;

namespace Colombo.Tests.Interceptors
{
    [TestFixture]
    [Category("PerfCounters")]
    public class PerfCounterFactoryTest : BaseTest
    {
        [Test]
        public void It_should_be_able_to_create_and_retrieve_performance_counters()
        {
            const string instanceName = "InstanceName";

            PerformanceCounterCategory.Delete(PerfCounterFactory.PerfCounterCategoryRequestHandling);
            PerformanceCounterCategory.Delete(PerfCounterFactory.PerfCounterCategoryMessageSending);

            var factory = new PerfCounterFactory {Logger = GetConsoleLogger()};
            Assert.That(factory.GetPerfCounter(PerfCounter.NumMessagesSent, instanceName),
                Is.Not.Null.And.Property("InstanceName").EqualTo(instanceName));
            Assert.That(factory.GetPerfCounter(PerfCounter.NumMessagesSentPerSec, instanceName),
                Is.Not.Null.And.Property("InstanceName").EqualTo(instanceName));
            Assert.That(factory.GetPerfCounter(PerfCounter.NumRequestsHandled, instanceName),
                Is.Not.Null.And.Property("InstanceName").EqualTo(instanceName));
            Assert.That(factory.GetPerfCounter(PerfCounter.NumRequestsHandledPerSec, instanceName),
                Is.Not.Null.And.Property("InstanceName").EqualTo(instanceName));

            Assert.That(factory.GetPerfCounter(PerfCounter.AverageDurationForMessageSending, instanceName),
                Is.Not.Null.And.Property("InstanceName").EqualTo(instanceName));
            Assert.That(factory.GetPerfCounter(PerfCounter.AverageDurationForMessageSendingBase, instanceName),
                Is.Not.Null.And.Property("InstanceName").EqualTo(instanceName));
            Assert.That(factory.GetPerfCounter(PerfCounter.AverageDurationForRequestHandling, instanceName),
                Is.Not.Null.And.Property("InstanceName").EqualTo(instanceName));
            Assert.That(factory.GetPerfCounter(PerfCounter.AverageDurationForRequestHandlingBase, instanceName),
                Is.Not.Null.And.Property("InstanceName").EqualTo(instanceName));
        }
    }
}
