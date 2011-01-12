using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Colombo.Alerts
{
    public class MemcachedUnreachableAlert : IColomboAlert
    {
        public MemcachedUnreachableAlert(string machineNameOrigin, string[] memcachedServers)
        {
            MachineNameOrigin = machineNameOrigin;
            MemcachedServers = memcachedServers;
        }

        public string MachineNameOrigin { get; private set; }

        public string[] MemcachedServers { get; private set; }

        public override string ToString()
        {
            return string.Format("Memcached servers in address(es) {0} may be unreachable from machine {1},",
                string.Join(", ", MemcachedServers),
                MachineNameOrigin);
        }
    }
}
