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
using System.Diagnostics.Contracts;
using System.Linq;
using Castle.Core.Logging;
using Colombo.Alerts;

namespace Colombo.Interceptors
{
    /// <summary>
    /// <see cref="IRequestHandlerHandleInterceptor"/> that sends <see cref="ExceptionAlert"/> when an exception occurs.
    /// </summary>
    public class ExceptionsHandleInterceptor : IRequestHandlerHandleInterceptor
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
                    Logger.Info("No alerters has been registered for Exceptions monitoring.");
                else
                    Logger.InfoFormat("Exceptions monitoring with the following alerters: {0}", string.Join(", ", alerters.Select(x => x.GetType().Name)));
            }
        }

        /// <summary>
        /// Alerts when exceptions.
        /// </summary>
        public void Intercept(IColomboRequestHandleInvocation nextInvocation)
        {
            try
            {
                nextInvocation.Proceed();
            }
            catch (Exception ex)
            {
                try
                {
                    var alert = new ExceptionAlert(new[] { nextInvocation.Request }, ex);
                    Logger.Warn(alert.ToString());
                    foreach (var alerter in Alerters)
                        alerter.Alert(alert);
                }
                catch
                {
                }
                throw;
            }
        }

        /// <summary>
        /// High
        /// </summary>
        public int InterceptionPriority
        {
            get { return InterceptionPrority.High; }
        }
    }
}
