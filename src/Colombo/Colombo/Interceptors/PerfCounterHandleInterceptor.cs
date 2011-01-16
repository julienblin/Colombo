using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using Castle.Core.Logging;

namespace Colombo.Interceptors
{
    /// <summary>
    /// Interceptor that monitor the performance of request handling through performance counters.
    /// Will create a specific performance counter instance per request group name.
    /// </summary>
    /// <seealso cref="PerfCounterSendInterceptor"/>
    public class PerfCounterHandleInterceptor : IRequestHandlerHandleInterceptor
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
        public void Intercept(IColomboRequestHandleInvocation invocation)
        {
            if (invocation == null) throw new ArgumentNullException("invocation");
            Contract.EndContractBlock();

            var watch = new Stopwatch();
            watch.Start();
            invocation.Proceed();
            watch.Stop();

            try
            {
                var instanceName = invocation.Request.GetGroupName();

                using (var numberOfRequestsHandled = PerfCounterFactory.GetPerfCounter(PerfCounter.NumRequestsHandled, instanceName))
                    numberOfRequestsHandled.Increment();

                using (var numberOfRequestsHandledPerSec = PerfCounterFactory.GetPerfCounter(PerfCounter.NumRequestsHandledPerSec, instanceName))
                    numberOfRequestsHandledPerSec.Increment();

                using (var averageDurationForRequestHandling = PerfCounterFactory.GetPerfCounter(PerfCounter.AverageDurationForRequestHandling, instanceName))
                    averageDurationForRequestHandling.IncrementBy(watch.ElapsedTicks);

                using (var averageDurationForRequestHandlingBase = PerfCounterFactory.GetPerfCounter(PerfCounter.AverageDurationForRequestHandlingBase, instanceName))
                    averageDurationForRequestHandlingBase.Increment();
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
