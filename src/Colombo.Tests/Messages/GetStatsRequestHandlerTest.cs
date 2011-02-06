using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Colombo.Messages;
using NUnit.Framework;
using Rhino.Mocks;

namespace Colombo.Tests.Messages
{
    [TestFixture]
    public class GetStatsRequestHandlerTest
    {
        [Test]
        public void It_should_return_response_if_stats_unavailable()
        {
            var handler = new GetStatsRequestHandler();
            var response = handler.Handle(new GetStatsRequest());

            Assert.That(response.StatsAvailable, Is.False);
        }

        [Test]
        public void It_should_use_stats_collector()
        {
            var mocks = new MockRepository();
            var collector = mocks.StrictMock<IColomboStatCollector>();
            var colomboStats = new ColomboStats
                                   {
                                       AverageTimePerRequestHandled = new TimeSpan(TimeSpan.TicksPerDay),
                                       NumRequestsHandled = 10,
                                       ColomboVersion = new Version(1,1),
                                       Uptime = new TimeSpan(TimeSpan.TicksPerMinute)
                                   };

            With.Mocks(mocks).Expecting(() =>
            {
                Expect.Call(collector.StatsAvailable).Return(true).Repeat.AtLeastOnce();
                Expect.Call(collector.GetStats()).Return(colomboStats);
            }).Verify(() =>
            {
                var handler = new GetStatsRequestHandler();
                handler.StatCollector = collector;
                var response = handler.Handle(new GetStatsRequest());

                Assert.That(response.StatsAvailable, Is.True);
                Assert.That(response.AverageTimePerRequestHandled, Is.EqualTo(colomboStats.AverageTimePerRequestHandled));
                Assert.That(response.NumRequestsHandled, Is.EqualTo(colomboStats.NumRequestsHandled));
                Assert.That(response.ColomboVersion, Is.EqualTo(colomboStats.ColomboVersion));
                Assert.That(response.Uptime, Is.EqualTo(colomboStats.Uptime));
            });
        }
    }
}
