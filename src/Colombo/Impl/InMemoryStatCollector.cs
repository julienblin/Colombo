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
using System.Threading;
using Castle.Core;

namespace Colombo.Impl
{
    /// <summary>
    /// Implementation of <see cref="IColomboStatCollector"/> that keeps stats in memory.
    /// </summary>
    public class InMemoryStatCollector : IColomboStatCollector, IStartable
    {
        private DateTime startTimeUtc;

        private int numRequestsHandled;

        private int numErrorsHandled;

        private long totalTicks;

        /// <summary>
        /// <c>true</c>
        /// </summary>
        public bool StatsAvailable
        {
            get { return true; }
        }

        /// <summary>
        /// Get the current statistics.
        /// </summary>
        /// <returns></returns>
        public ColomboStats GetStats()
        {
            return new ColomboStats
                       {
                           Uptime = (DateTime.UtcNow - startTimeUtc),
                           ColomboVersion = typeof(IMessageBus).Assembly.GetName().Version,
                           NumRequestsHandled = numRequestsHandled,
                           NumErrors = numErrorsHandled,
                           AverageTimePerRequestHandled = totalTicks == 0 ? TimeSpan.Zero : new TimeSpan(totalTicks / numRequestsHandled),
                           ErrorRate = numErrorsHandled == 0 ? 0 : ((numErrorsHandled / Convert.ToDecimal(numRequestsHandled + numErrorsHandled)) * 100m)
                       };
        }

        /// <summary>
        /// Increments the count for requests handled. Thread-safe.
        /// </summary>
        /// <param name="numRequests">Number of requests to increment.</param>
        /// <param name="timeSpent">Time spent to handle all the requests.</param>
        public void IncrementRequestsHandled(int numRequests, TimeSpan timeSpent)
        {
            Interlocked.Add(ref numRequestsHandled, numRequests);
            Interlocked.Add(ref totalTicks, timeSpent.Ticks * numRequests);
        }

        /// <summary>
        /// Increments the count for errors (Exceptions) in handled requests. Thread-safe.
        /// </summary>
        /// <param name="numErrors">Number of errors to increment.</param>
        public void IncrementErrors(int numErrors)
        {
            Interlocked.Add(ref numErrorsHandled, numErrors);
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
