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

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Colombo.Impl.NotificationHandle;
using Colombo.Impl.Notify;

namespace Colombo.TestSupport
{
    internal class StubNotifyInvocation : BaseNotifyInvocation
    {
        private readonly IStubMessageBus stubMessageBus;
        private readonly INotificationHandleInterceptor[] notificationHandleInterceptors;

        public StubNotifyInvocation(IStubMessageBus stubMessageBus, INotificationHandleInterceptor[] notificationHandleInterceptors)
        {
            this.stubMessageBus = stubMessageBus;
            this.notificationHandleInterceptors = notificationHandleInterceptors;
        }

        public override void Proceed()
        {
            foreach (var invoc in BuildHandleInvocationChains(Notifications))
                Task.Factory.StartNew(i => ((IColomboNotificationHandleInvocation)i).Proceed(), invoc);
        }

        private IEnumerable<IColomboNotificationHandleInvocation> BuildHandleInvocationChains(IEnumerable<Notification> notifications)
        {
            foreach (var notification in notifications)
            {
                IColomboNotificationHandleInvocation currentInvocation = new StubNotificationHandleInvocation(this.stubMessageBus);
                foreach (var interceptor in notificationHandleInterceptors.Reverse())
                {
                    if (interceptor != null)
                        currentInvocation = new NotificationHandleInterceptorInvocation(interceptor, currentInvocation);
                }
                currentInvocation.Notification = notification;
                yield return currentInvocation;
            }
        }
    }
}
