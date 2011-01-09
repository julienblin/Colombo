using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Castle.Core.Logging;
using System.Diagnostics.Contracts;

namespace Colombo.Interceptors
{
    public class PerfCounterHandleInterceptor : IRequestHandlerHandleInterceptor
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

        public void Intercept(IColomboHandleInvocation invocation)
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

        public int InterceptionPriority
        {
            get { return InterceptorPrority.ReservedHigh; }
        }
    }
}
