using System;
using System.Diagnostics;
using Castle.Core.Logging;

namespace Colombo.Interceptors
{
    /// <summary>
    /// Factory that creates and returns performance counters
    /// </summary>
    public class PerfCounterFactory
    {
        /// <summary>
        /// Performance counters category for the counters related to request handling.
        /// </summary>
        public const string PerfCounterCategoryRequestHandling = @"Colombo - Requests Handling";

        /// <summary>
        /// Performance counters category for the counters related to message sending.
        /// </summary>
        public const string PerfCounterCategoryMessageSending = @"Colombo - Messages Sending";

        private ILogger logger = NullLogger.Instance;
        /// <summary>
        /// Logger
        /// </summary>
        public ILogger Logger
        {
            get { return logger; }
            set { logger = value; }
        }

        bool testPerfCountersCreated;

        /// <summary>
        /// Returns a performance counter.
        /// </summary>
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

        /// <summary>
        /// Create the necessary performance counters in the system.
        /// Must be run with administrator privileges.
        /// </summary>
        public static void CreatePerfCounters()
        {
            if (!PerformanceCounterCategory.Exists(PerfCounterCategoryRequestHandling))
            {
                var counters = new CounterCreationDataCollection();

                var numberOfRequestsHandled = new CounterCreationData
                                                  {
                                                      CounterName = @"# requests handled",
                                                      CounterHelp = @"Total number of requests handled",
                                                      CounterType = PerformanceCounterType.NumberOfItems32
                                                  };
                counters.Add(numberOfRequestsHandled);

                var numberOfRequestsHandledPerSec = new CounterCreationData
                                                        {
                                                            CounterName = @"# requests handled / sec",
                                                            CounterHelp = @"Number of requests handled per second",
                                                            CounterType = PerformanceCounterType.RateOfCountsPerSecond32
                                                        };
                counters.Add(numberOfRequestsHandledPerSec);

                var averageDurationForRequestHandling = new CounterCreationData
                                                            {
                                                                CounterName = @"average time per request handling",
                                                                CounterHelp =
                                                                    @"Average time spent handling (processing) a request",
                                                                CounterType = PerformanceCounterType.AverageTimer32
                                                            };
                counters.Add(averageDurationForRequestHandling);

                var averageDurationForRequestHandlingBase = new CounterCreationData
                                                                {
                                                                    CounterName =
                                                                        @"average time per request handling base",
                                                                    CounterHelp =
                                                                        @"Average time spent handling (processing) a request base",
                                                                    CounterType = PerformanceCounterType.AverageBase
                                                                };
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

                var numberOfMessageSent = new CounterCreationData
                                              {
                                                  CounterName = @"# messages sent",
                                                  CounterHelp = @"Total number of messages sent",
                                                  CounterType = PerformanceCounterType.NumberOfItems32
                                              };
                counters.Add(numberOfMessageSent);

                var numberOfMessageSentPerSec = new CounterCreationData
                                                    {
                                                        CounterName = @"# messages sent / sec",
                                                        CounterHelp = @"Number of messages sent per second",
                                                        CounterType = PerformanceCounterType.RateOfCountsPerSecond32
                                                    };
                counters.Add(numberOfMessageSentPerSec);

                var averageDurationForMessageSending = new CounterCreationData
                                                           {
                                                               CounterName = @"average time per message sending",
                                                               CounterHelp = @"Average time spent sending messages",
                                                               CounterType = PerformanceCounterType.AverageTimer32
                                                           };
                counters.Add(averageDurationForMessageSending);

                var averageDurationForMessageSendingBase = new CounterCreationData
                                                               {
                                                                   CounterName =
                                                                       @"average time per message sending base",
                                                                   CounterHelp =
                                                                       @"Average time spent sending messages base",
                                                                   CounterType = PerformanceCounterType.AverageBase
                                                               };
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

    /// <summary>
    /// Performance counters
    /// </summary>
    public enum PerfCounter
    {
        /// <summary>
        /// Total number of requests handled
        /// </summary>
        NumRequestsHandled,

        /// <summary>
        /// Total number of requests handled per second
        /// </summary>
        NumRequestsHandledPerSec,

        /// <summary>
        /// Average time spent handling (processing) a request
        /// </summary>
        AverageDurationForRequestHandling,

        /// <summary>
        /// Average time spent handling (processing) a request base
        /// </summary>
        AverageDurationForRequestHandlingBase,

        /// <summary>
        /// Total number of messages sent
        /// </summary>
        NumMessagesSent,

        /// <summary>
        /// Number of messages sent per second
        /// </summary>
        NumMessagesSentPerSec,

        /// <summary>
        /// Average time spent sending messages
        /// </summary>
        AverageDurationForMessageSending,

        /// <summary>
        /// Average time spent sending messages base
        /// </summary>
        AverageDurationForMessageSendingBase
    }
}
