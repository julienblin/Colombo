using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Colombo.Impl;

namespace Colombo.Messages
{
    public class GetStatsRequestHandler : SideEffectFreeRequestHandler<GetStatsRequest, GetStatsResponse>
    {
        private IColomboStatCollector statCollector = NullStatCollector.Instance;
        /// <summary>
        /// Stats collector.
        /// </summary>
        public IColomboStatCollector StatCollector
        {
            get { return statCollector; }
            set { statCollector = value; }
        }

        /// <summary>
        /// Handles the request.
        /// </summary>
        protected override void Handle()
        {
            Response.StatsAvailable = StatCollector.StatsAvailable;
            if (!StatCollector.StatsAvailable) return;

            var stats = StatCollector.GetStats();
            if (stats == null) return;

            Response.Uptime = stats.Uptime;
            Response.ColomboVersion = stats.ColomboVersion.ToString();
            Response.NumRequestsHandled = stats.NumRequestsHandled;
            Response.AverageTimePerRequestHandled = stats.AverageTimePerRequestHandled;
        }
    }
}
