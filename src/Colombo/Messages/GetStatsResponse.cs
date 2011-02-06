using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Colombo.Messages
{
    public class GetStatsResponse : Response
    {
        public virtual bool StatsAvailable { get; set; }

        public virtual TimeSpan Uptime { get; set; }

        public virtual int NumRequestsHandled { get; set; }

        public virtual TimeSpan AverageTimePerRequestHandled { get; set; }

        public virtual string ColomboVersion { get; set; }
    }
}
