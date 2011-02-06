using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Colombo
{
    public interface IColomboStatCollector
    {
        bool StatsAvailable { get; }

        ColomboStats GetStats();

        void IncrementRequestsHandled(int numRequests, TimeSpan timeSpent);
    }
}
