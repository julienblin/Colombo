using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Castle.Core.Logging;

namespace Colombo.Interceptors
{
    public class PerfCounterFactory
    {
        public const string PerfCounterCategoryRequestHandling = @"Colombo - Requests Handling";
        public const string PerfCounterCategoryMessageSending = @"Colombo - Messages Sending";

        private ILogger logger = NullLogger.Instance;
        public ILogger Logger
        {
            get { return logger; }
            set { logger = value; }
        }

        bool testPerfCountersCreated = false;

        public PerformanceCounter GetPerfCounter(PerfCounter counter, string instanceName, bool readOnly = false)
        {
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

            var perfCounter = new PerformanceCounter();

            switch (counter)
            {
                case PerfCounter.NumRequestsHandled:
                    perfCounter.CounterName = @"# requests handled";
                    perfCounter.CategoryName = PerfCounterCategoryRequestHandling;
                    break;
                case PerfCounter.NumRequestsHandledPerSec:
                    perfCounter.CounterName = @"# requests handled / sec";
                    perfCounter.CategoryName = PerfCounterCategoryRequestHandling;
                    break;
                case PerfCounter.AverageDurationForRequestHandling:
                    perfCounter.CounterName = @"average time per request handling";
                    perfCounter.CategoryName = PerfCounterCategoryRequestHandling;
                    break;
                case PerfCounter.AverageDurationForRequestHandlingBase:
                    perfCounter.CounterName = @"average time per request handling base";
                    perfCounter.CategoryName = PerfCounterCategoryRequestHandling;
                    break;
                case PerfCounter.NumMessagesSent:
                    perfCounter.CounterName = @"# messages sent";
                    perfCounter.CategoryName = PerfCounterCategoryMessageSending;
                    break;
                case PerfCounter.NumMessagesSentPerSec:
                    perfCounter.CounterName = @"# messages sent / sec";
                    perfCounter.CategoryName = PerfCounterCategoryMessageSending;
                    break;
                case PerfCounter.AverageDurationForMessageSending:
                    perfCounter.CounterName = @"average time per message sending";
                    perfCounter.CategoryName = PerfCounterCategoryMessageSending;
                    break;
                case PerfCounter.AverageDurationForMessageSendingBase:
                    perfCounter.CounterName = @"average time per message sending base";
                    perfCounter.CategoryName = PerfCounterCategoryMessageSending;
                    break;
                default:
                    throw new ColomboException(string.Format("Unable to create performance counter {0}", counter));
            }

            perfCounter.MachineName = ".";
            perfCounter.InstanceName = instanceName;
            perfCounter.ReadOnly = readOnly;
            return perfCounter;
        }

        public static void CreatePerfCounters()
        {
            if (!PerformanceCounterCategory.Exists(PerfCounterCategoryRequestHandling))
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

                var averageDurationForRequestHandling = new CounterCreationData();
                averageDurationForRequestHandling.CounterName = @"average time per request handling";
                averageDurationForRequestHandling.CounterHelp = @"Average time spent handling (processing) a request";
                averageDurationForRequestHandling.CounterType = PerformanceCounterType.AverageTimer32;
                counters.Add(averageDurationForRequestHandling);

                var averageDurationForRequestHandlingBase = new CounterCreationData();
                averageDurationForRequestHandlingBase.CounterName = @"average time per request handling base";
                averageDurationForRequestHandlingBase.CounterHelp = @"Average time spent handling (processing) a request base";
                averageDurationForRequestHandlingBase.CounterType = PerformanceCounterType.AverageBase;
                counters.Add(averageDurationForRequestHandlingBase);

                PerformanceCounterCategory.Create(
                    PerfCounterCategoryRequestHandling,
                    "Counters for the Colombo messaging framework related to request handling",
                    PerformanceCounterCategoryType.MultiInstance,
                    counters
                );
            }

            if (!PerformanceCounterCategory.Exists(PerfCounterCategoryMessageSending))
            {
                var counters = new CounterCreationDataCollection();

                var numberOfMessageSent = new CounterCreationData();
                numberOfMessageSent.CounterName = @"# messages sent";
                numberOfMessageSent.CounterHelp = @"Total number of messages sent";
                numberOfMessageSent.CounterType = PerformanceCounterType.NumberOfItems32;
                counters.Add(numberOfMessageSent);

                var numberOfMessageSentPerSec = new CounterCreationData();
                numberOfMessageSentPerSec.CounterName = @"# messages sent / sec";
                numberOfMessageSentPerSec.CounterHelp = @"Number of messages sent per second";
                numberOfMessageSentPerSec.CounterType = PerformanceCounterType.RateOfCountsPerSecond32;
                counters.Add(numberOfMessageSentPerSec);

                var averageDurationForMessageSending = new CounterCreationData();
                averageDurationForMessageSending.CounterName = @"average time per message sending";
                averageDurationForMessageSending.CounterHelp = @"Average time spent sending messages";
                averageDurationForMessageSending.CounterType = PerformanceCounterType.AverageTimer32;
                counters.Add(averageDurationForMessageSending);

                var averageDurationForMessageSendingBase = new CounterCreationData();
                averageDurationForMessageSendingBase.CounterName = @"average time per message sending base";
                averageDurationForMessageSendingBase.CounterHelp = @"Average time spent sending messages base";
                averageDurationForMessageSendingBase.CounterType = PerformanceCounterType.AverageBase;
                counters.Add(averageDurationForMessageSendingBase);

                PerformanceCounterCategory.Create(
                    PerfCounterCategoryMessageSending,
                    "Counters for the Colombo messaging framework related to message sending",
                    PerformanceCounterCategoryType.MultiInstance,
                    counters
                );
            }
        }
    }

    public enum PerfCounter
    {
        NumRequestsHandled,
        NumRequestsHandledPerSec,
        AverageDurationForRequestHandling,
        AverageDurationForRequestHandlingBase,

        NumMessagesSent,
        NumMessagesSentPerSec,
        AverageDurationForMessageSending,
        AverageDurationForMessageSendingBase
    }
}
