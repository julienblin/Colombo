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

        [Test]
        public void It_should_report_num_errors()
        {
            var statCollector = new InMemoryStatCollector();

            var stats = statCollector.GetStats();
            Assert.That(stats.NumErrors, Is.EqualTo(0));

            statCollector.IncrementErrors(2);
            statCollector.IncrementErrors(4);

            stats = statCollector.GetStats();
            Assert.That(stats.NumErrors, Is.EqualTo(6));
        }

        [Test]
        public void It_should_report_error_rate()
        {
            var statCollector = new InMemoryStatCollector();

            var stats = statCollector.GetStats();
            Assert.That(stats.ErrorRate, Is.EqualTo(0));

            statCollector.IncrementErrors(5);
            statCollector.IncrementRequestsHandled(15, TimeSpan.Zero);

            stats = statCollector.GetStats();
            Assert.That(stats.ErrorRate, Is.EqualTo(25m));
        }
    }
}
