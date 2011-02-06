using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Castle.Core;
using Colombo.Impl;
using NUnit.Framework;

namespace Colombo.Tests.Impl
{
    [TestFixture]
    public class InMemoryStatCollectorTest
    {
        [Test]
        public void It_should_report_uptime()
        {
            var statCollector = new InMemoryStatCollector();
            ((IStartable)statCollector).Start();

            Thread.Sleep(100);

            var stats = statCollector.GetStats();
            Assert.That(stats.Uptime, Is.GreaterThan(new TimeSpan(0, 0, 0, 0, 90)));
        }

        [Test]
        public void It_should_report_Colombo_version()
        {
            var statCollector = new InMemoryStatCollector();

            var stats = statCollector.GetStats();

            Assert.That(stats.ColomboVersion, Is.EqualTo(typeof(IMessageBus).Assembly.GetName().Version));
        }

        [Test]
        public void It_should_report_num_requests_handled()
        {
            var statCollector = new InMemoryStatCollector();

            var stats = statCollector.GetStats();
            Assert.That(stats.NumRequestsHandled, Is.EqualTo(0));

            statCollector.IncrementRequestsHandled(3, new TimeSpan(0, 0, 0, 0, 200));
            statCollector.IncrementRequestsHandled(2, new TimeSpan(0, 0, 0, 0, 500));

            stats = statCollector.GetStats();
            Assert.That(stats.NumRequestsHandled, Is.EqualTo(5));
        }

        [Test]
        public void It_should_report_average_time_per_request_handled()
        {
            var statCollector = new InMemoryStatCollector();

            var stats = statCollector.GetStats();
            Assert.That(stats.NumRequestsHandled, Is.EqualTo(0));

            statCollector.IncrementRequestsHandled(3, new TimeSpan(0, 0, 0, 0, 200));
            statCollector.IncrementRequestsHandled(2, new TimeSpan(0, 0, 0, 0, 500));

            stats = statCollector.GetStats();
            Assert.That(stats.AverageTimePerRequestHandled, Is.EqualTo(new TimeSpan(0, 0, 0, 0, 320)));
        }
    }
}
