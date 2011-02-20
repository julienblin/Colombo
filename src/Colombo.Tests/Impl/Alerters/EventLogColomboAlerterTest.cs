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
