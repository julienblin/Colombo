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
