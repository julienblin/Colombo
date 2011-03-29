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
using System.Linq;
using Colombo.Messages;
using Colombo.TestSupport;
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
                                       NumErrors = 5,
                                       Uptime = new TimeSpan(TimeSpan.TicksPerMinute),
                                       ErrorRate = 50m
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
                Assert.That(response.NumErrors, Is.EqualTo(colomboStats.NumErrors));
                Assert.That(response.Uptime, Is.EqualTo(colomboStats.Uptime));
                Assert.That(response.ErrorRate, Is.EqualTo(colomboStats.ErrorRate));
            });
        }

        [Test]
        public void It_should_return_a_list_of_loaded_assemblies()
        {
            var handler = new GetStatsRequestHandler();
            var response = handler.Handle(new GetStatsRequest());

            Assert.That(response.Assemblies.Any(x => x.Equals(GetType().Assembly.GetName().FullName)));

            Assert.That(!response.Assemblies.Any(x => x.StartsWith("System")));
            Assert.That(!response.Assemblies.Any(x => x.StartsWith("mscorlib")));
        }
    }
}
