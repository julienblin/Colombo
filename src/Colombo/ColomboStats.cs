using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Colombo
{
    public class ColomboStats
    {
        public TimeSpan Uptime { get; set; }

        public int NumRequestsHandled { get; set; }

        public TimeSpan AverageTimePerRequestHandled { get; set; }

        public Version ColomboVersion { get; set; }
    }
}
