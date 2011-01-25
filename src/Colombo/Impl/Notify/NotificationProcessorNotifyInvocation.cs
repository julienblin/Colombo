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
using System.Threading.Tasks;

namespace Colombo.Impl.Notify
{
    internal class NotificationProcessorNotifyInvocation : BaseNotifyInvocation
    {
        private readonly INotificationProcessor[] notificationProcessors;

        public NotificationProcessorNotifyInvocation(INotificationProcessor[] notificationProcessors)
        {
            if (notificationProcessors == null) throw new ArgumentNullException("notificationProcessors");
            Contract.EndContractBlock();

            this.notificationProcessors = notificationProcessors;
        }

        public override void Proceed()
        {
            var finalNotificationsArray = Notifications.ToArray();
            foreach (var processor in notificationProcessors.Where(processor => processor != null))
            {
                Task.Factory.StartNew(proc => ((INotificationProcessor)proc).Process(finalNotificationsArray), processor);
            }
        }
    }
}
