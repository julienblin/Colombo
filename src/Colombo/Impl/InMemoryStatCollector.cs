using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Castle.Core;

namespace Colombo.Impl
{
    public class InMemoryStatCollector : IColomboStatCollector, IStartable
    {
        private DateTime startTimeUtc;

        private int numRequestsHandled;

        private long totalTicks;

        public bool StatsAvailable
        {
            get { return true; }
        }

        public ColomboStats GetStats()
        {
            return new ColomboStats
                       {
                           Uptime = (DateTime.UtcNow - startTimeUtc),
                           ColomboVersion = typeof(IMessageBus).Assembly.GetName().Version,
                           NumRequestsHandled = numRequestsHandled,
                           AverageTimePerRequestHandled = totalTicks == 0 ? new TimeSpan(0) : new TimeSpan(totalTicks / numRequestsHandled)
                       };
        }

        public void IncrementRequestsHandled(int numRequests, TimeSpan timeSpent)
        {
            Interlocked.Add(ref numRequestsHandled, numRequests);
            Interlocked.Add(ref totalTicks, timeSpent.Ticks * numRequests);
        }

        /// <summary>
        /// Starts this instance.
        /// </summary>
        void IStartable.Start()
        {
            startTimeUtc = DateTime.UtcNow;
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        void IStartable.Stop()
        {
            
        }
    }
}
