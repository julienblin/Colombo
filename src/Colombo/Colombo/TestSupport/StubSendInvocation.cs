﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Colombo.Impl.Send;

namespace Colombo.TestSupport
{
    internal class StubSendInvocation : BaseSendInvocation
    {
        private readonly IStubMessageBus stubMessageBus;

        public StubSendInvocation(IStubMessageBus stubMessageBus)
        {
            this.stubMessageBus = stubMessageBus;
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
            return currentInvocation;
        }
    }
}