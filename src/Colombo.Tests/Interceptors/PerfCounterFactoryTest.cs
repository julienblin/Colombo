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

            try
            {
                PerformanceCounterCategory.Delete(PerfCounterFactory.PerfCounterCategoryRequestHandling);
                PerformanceCounterCategory.Delete(PerfCounterFactory.PerfCounterCategoryMessageSending);
            }
            catch { }

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
