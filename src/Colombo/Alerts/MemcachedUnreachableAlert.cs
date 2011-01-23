namespace Colombo.Alerts
{
    /// <summary>
    /// This alert means that a Memcached server was unreachable.
    /// </summary>
    public class MemcachedUnreachableAlert : IColomboAlert
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="machineNameOrigin">Name of machine from which the Colombo client executes (source).</param>
        /// <param name="memcachedServers">Addresses of the memcached servers. (target)</param>
        public MemcachedUnreachableAlert(string machineNameOrigin, string[] memcachedServers)
        {
            MachineNameOrigin = machineNameOrigin;
            MemcachedServers = memcachedServers;
        }

        /// <summary>
        /// Name of machine from which the Colombo client executes (source).
        /// </summary>
        public string MachineNameOrigin { get; private set; }

        /// <summary>
        /// Addresses of the memcached servers. (target). Only one of these servers might be down.
        /// </summary>
        public string[] MemcachedServers { get; private set; }

        /// <summary>
        /// A description of the alert.
        /// </summary>
        public override string ToString()
        {
            return string.Format("Memcached servers in address(es) {0} may be unreachable from machine {1},",
                string.Join(", ", MemcachedServers),
                MachineNameOrigin);
        }
    }
}
