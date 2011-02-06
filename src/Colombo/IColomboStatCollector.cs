using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Colombo
{
    public interface IColomboStatCollector
    {
        ColomboStats GetStats();

        void IncrementRequestsHandled(int numRequests, TimeSpan timeSpent);
    }
}
