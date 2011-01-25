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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Colombo.Impl.RequestHandle;
using Colombo.Impl.Send;

namespace Colombo.TestSupport
{
    internal class StubSendInvocation : BaseSendInvocation
    {
        private readonly IStubMessageBus stubMessageBus;
        private readonly IRequestHandlerHandleInterceptor[] requestHandlerInterceptors;

        public StubSendInvocation(IStubMessageBus stubMessageBus, IRequestHandlerHandleInterceptor[] requestHandlerInterceptors)
        {
            this.stubMessageBus = stubMessageBus;
            this.requestHandlerInterceptors = requestHandlerInterceptors;
        }

        public override void Proceed()
        {
            var tasks = new List<Task<Response>>();
            var tasksRequestAssociation = new Dictionary<BaseRequest, Task<Response>>();
            foreach (var request in Requests)
            {
                var localRequest = request;
                var task = Task.Factory.StartNew(() =>
                {
                    var topInvocation = BuildHandleInvocationChain();
                    topInvocation.Request = localRequest;
                    topInvocation.Proceed();
                    return topInvocation.Response;                                
                });
                tasks.Add(task);
                tasksRequestAssociation[request] = task;
            }

            try
            {
                Task.WaitAll(tasks.ToArray());
            }
            catch (AggregateException ex)
            {
                if ((ex.InnerExceptions.Count == 1) && (ex.InnerExceptions[0] is ColomboExpectationException))
                    throw ex.InnerExceptions[0];
                else
                    throw new ColomboException("An exception occured inside one or several request handlers", ex);
            }

            if (Responses == null)
                Responses = new ResponsesGroup();

            foreach (var request in Requests)
                Responses[request] = tasksRequestAssociation[request].Result;
        }

        public IColomboRequestHandleInvocation BuildHandleInvocationChain()
        {
            IColomboRequestHandleInvocation currentInvocation = new StubRequestHandleInvocation(stubMessageBus);
            foreach (var interceptor in requestHandlerInterceptors.Reverse())
            {
                if (interceptor != null)
                    currentInvocation = new RequestHandlerHandleInterceptorInvocation(interceptor, currentInvocation);
            }
            return currentInvocation;
        }
    }
}
