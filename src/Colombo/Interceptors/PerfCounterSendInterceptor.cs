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
using System.Linq;
using Castle.Core.Logging;

namespace Colombo.Interceptors
{
    /// <summary>
    /// Interceptor that monitor the performance of message sending through performance counters.
    /// Will create a specific performance counter instance per request group name.
    /// </summary>
    /// <seealso cref="PerfCounterHandleInterceptor"/>
    public class PerfCounterSendInterceptor : IMessageBusSendInterceptor
    {
        private ILogger logger = NullLogger.Instance;
        /// <summary>
        /// Logger
        /// </summary>
        public ILogger Logger
        {
            get { return logger; }
            set { logger = value; }
        }

        private PerfCounterFactory perfCounterFactory;

        private PerfCounterFactory PerfCounterFactory
        {
            get { return perfCounterFactory ?? (perfCounterFactory = new PerfCounterFactory {Logger = Logger}); }
        }

        /// <summary>
        /// Monitor the performance.
        /// </summary>
        public void Intercept(IColomboSendInvocation invocation)
        {
            if (invocation == null) throw new ArgumentNullException("invocation");
            Contract.EndContractBlock();

            var watch = new Stopwatch();
            watch.Start();
            invocation.Proceed();
            watch.Stop();

            try
            {
                Contract.Assume(invocation.Requests != null);
                var instancesGroups = invocation.Requests.GroupBy(x => x.GetGroupName());

                foreach (var instanceGroups in instancesGroups)
                {
                    var instanceName = instanceGroups.Key;

                    using (var numMessagesSent = PerfCounterFactory.GetPerfCounter(PerfCounter.NumMessagesSent, instanceName))
                        numMessagesSent.IncrementBy(instanceGroups.LongCount());

                    using (var numMessagesSentPerSec = PerfCounterFactory.GetPerfCounter(PerfCounter.NumMessagesSentPerSec, instanceName))
                        numMessagesSentPerSec.IncrementBy(instanceGroups.LongCount());

                    using (var averageDurationForMessageSending = PerfCounterFactory.GetPerfCounter(PerfCounter.AverageDurationForMessageSending, instanceName))
                        averageDurationForMessageSending.IncrementBy(watch.ElapsedTicks);

                    using (var averageDurationForMessageSendingBase = PerfCounterFactory.GetPerfCounter(PerfCounter.AverageDurationForMessageSendingBase, instanceName))
                        averageDurationForMessageSendingBase.IncrementBy(instanceGroups.LongCount());
                }
            }
            catch (Exception ex)
            {
                Logger.Warn("Error while computing performance counters values.", ex);
            }
        }

        /// <summary>
        /// Really high - runs first.
        /// </summary>
        public int InterceptionPriority
        {
            get { return InterceptionPrority.ReservedHigh; }
        }
    }
}
