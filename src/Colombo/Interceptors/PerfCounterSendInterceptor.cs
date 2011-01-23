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
