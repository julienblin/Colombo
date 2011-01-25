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
