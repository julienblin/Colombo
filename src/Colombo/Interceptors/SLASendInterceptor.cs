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
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using Castle.Core.Logging;
using Colombo.Alerts;

namespace Colombo.Interceptors
{
    /// <summary>
    /// Interceptor that times and validates requests marked with <see cref="SLAAttribute"/>.
    /// It will also print the elapsed send time in Log.Debug.
    /// Uses <see cref="IColomboAlerter"/> to emit <see cref="Colombo.Alerts.SLABreachedAlert"/> if the requests takes more time than specified.
    /// </summary>
    public class SLASendInterceptor : IMessageBusSendInterceptor
    {
        private ILogger logger = NullLogger.Instance;
        /// <summary>
        /// Logger.
        /// </summary>
        public ILogger Logger
        {
            get { return logger; }
            set { logger = value; }
        }

        private IColomboAlerter[] alerters = new IColomboAlerter[0];
        /// <summary>
        /// Alerters to use. All will be notified.
        /// </summary>
        public IColomboAlerter[] Alerters
        {
            get { return alerters; }
            set
            {
                if (value == null) throw new ArgumentNullException("Alerters");
                Contract.EndContractBlock();

                alerters = value;

                if (!Logger.IsInfoEnabled) return;

                if (alerters.Length == 0)
                    Logger.Info("No alerters has been registered for SLA monitoring.");
                else
                    Logger.InfoFormat("SLA monitoring with the following alerters: {0}", string.Join(", ", alerters.Select(x => x.GetType().Name)));
            }
        }

        /// <summary>
        /// Measure the time spent and raise alerts.
        /// </summary>
        public void Intercept(IColomboSendInvocation invocation)
        {
            if (invocation == null) throw new ArgumentNullException("invocation");
            Contract.EndContractBlock();

            var watch = new Stopwatch();
            watch.Start();
            invocation.Proceed();
            watch.Stop();
            Logger.DebugFormat("{0} requests took {1} ms.", invocation.Requests.Count, watch.ElapsedMilliseconds);

            var maxSla = TimeSpan.MinValue;

            foreach (var request in invocation.Requests)
            {
                var slaAttribute = request.GetCustomAttribute<SLAAttribute>();
                if (slaAttribute == null) continue;

                if (maxSla < slaAttribute.Allowed)
                    maxSla = slaAttribute.Allowed;
            }

            if (maxSla == TimeSpan.MinValue) return;
            if (watch.Elapsed <= maxSla) return;

            var alert = new SLABreachedAlert(invocation.Requests.ToArray(), maxSla, watch.Elapsed);
            Logger.Warn(alert.ToString());
            foreach (var alerter in Alerters)
            {
                alerter.Alert(alert);
            }
        }

        /// <summary>
        /// Really high - runs firt.
        /// </summary>
        public int InterceptionPriority
        {
            get { return InterceptionPrority.ReservedHigh; }
        }
    }
}
