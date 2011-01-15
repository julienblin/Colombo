using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using Castle.Core.Logging;

namespace Colombo.Interceptors
{
    public class PerfCounterSendInterceptor : IMessageBusSendInterceptor
    {
        private ILogger logger = NullLogger.Instance;
        public ILogger Logger
        {
            get { return logger; }
            set { logger = value; }
        }

        private PerfCounterFactory perfCounterFactory;

        protected PerfCounterFactory PerfCounterFactory
        {
            get
            {
                if (perfCounterFactory == null)
                {
                    perfCounterFactory = new PerfCounterFactory();
                    perfCounterFactory.Logger = Logger;
                }
                return perfCounterFactory;
            }
        }

        [System.Runtime.InteropServices.DllImport("Kernel32.dll")]
        private static extern void QueryPerformanceCounter(ref long ticks);

        public void Intercept(IColomboSendInvocation invocation)
        {
            if (invocation == null) throw new ArgumentNullException("nextInvocation");
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

        public int InterceptionPriority
        {
            get { return InterceptorPrority.ReservedHigh; }
        }
    }
}
