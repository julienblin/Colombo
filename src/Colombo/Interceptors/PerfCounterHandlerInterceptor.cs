using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Castle.Core.Logging;
using System.Diagnostics.Contracts;

namespace Colombo.Interceptors
{
    public class PerfCounterHandlerInterceptor : IRequestHandlerInterceptor
    {
        public const string PerfCounterCategory = @"Colombo";

        private ILogger logger = NullLogger.Instance;
        public ILogger Logger
        {
            get { return logger; }
            set { logger = value; }
        }

        bool testPerfCountersCreated = false;

        public void Intercept(IColomboInvocation invocation)
        {
            if (invocation == null) throw new ArgumentNullException("invocation");
            Contract.EndContractBlock();

            if (!testPerfCountersCreated)
            {
                try
                {
                    CreatePerfCounters();
                }
                catch (Exception ex)
                {
                    Logger.Warn("Unable to create performance counters.", ex);
                }
                testPerfCountersCreated = true;
            }

            invocation.Proceed();

            try
            {
                var instanceName = invocation.Request.GetGroupName();
                using (var numberOfRequestsHandled = new PerformanceCounter())
                {
                    numberOfRequestsHandled.CategoryName = PerfCounterCategory;
                    numberOfRequestsHandled.CounterName = @"# requests handled";
                    numberOfRequestsHandled.MachineName = ".";
                    numberOfRequestsHandled.InstanceName = instanceName;
                    numberOfRequestsHandled.ReadOnly = false;

                    numberOfRequestsHandled.Increment();
                }

                using (var numberOfRequestsHandledPerSec = new PerformanceCounter())
                {
                    numberOfRequestsHandledPerSec.CategoryName = PerfCounterCategory;
                    numberOfRequestsHandledPerSec.CounterName = @"# requests handled / sec";
                    numberOfRequestsHandledPerSec.MachineName = ".";
                    numberOfRequestsHandledPerSec.InstanceName = instanceName;
                    numberOfRequestsHandledPerSec.ReadOnly = false;

                    numberOfRequestsHandledPerSec.Increment();
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

        public static void CreatePerfCounters()
        {
            if (!PerformanceCounterCategory.Exists(PerfCounterCategory))
            {
                var counters = new CounterCreationDataCollection();

                var numberOfRequestsHandled = new CounterCreationData();
                numberOfRequestsHandled.CounterName = @"# requests handled";
                numberOfRequestsHandled.CounterHelp = @"Total number of requests handled";
                numberOfRequestsHandled.CounterType = PerformanceCounterType.NumberOfItems32;
                counters.Add(numberOfRequestsHandled);

                var numberOfRequestsHandledPerSec = new CounterCreationData();
                numberOfRequestsHandledPerSec.CounterName = @"# requests handled / sec";
                numberOfRequestsHandledPerSec.CounterHelp = @"Number of requests handled per second";
                numberOfRequestsHandledPerSec.CounterType = PerformanceCounterType.RateOfCountsPerSecond32;
                counters.Add(numberOfRequestsHandledPerSec);

                //var avgDuration = new CounterCreationData();
                //avgDuration.CounterName = @"average time per request handled";
                //avgDuration.CounterHelp = @"Average duration per request handling";
                //avgDuration.CounterType = PerformanceCounterType.AverageTimer32;
                //counters.Add(avgDuration);

                PerformanceCounterCategory.Create(
                    PerfCounterCategory,
                    "Counters for the Colombo messaging framework",
                    PerformanceCounterCategoryType.MultiInstance,
                    counters
                );
            }
        }
    }
}
