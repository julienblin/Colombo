using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Colombo.Impl
{
    public class NullStatCollector : IColomboStatCollector
    {
        public static readonly NullStatCollector Instance = new NullStatCollector();

        public bool StatsAvailable
        {
            get { return false; }
        }

        public ColomboStats GetStats()
        {
            return null;
        }

        public void IncrementRequestsHandled(int numRequests, TimeSpan timeSpent)
        {
            
        }
    }
}
