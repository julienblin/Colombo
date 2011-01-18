using System;
using Colombo.Impl.RequestHandle;

namespace Colombo.TestSupport
{
    internal class StubRequestHandleInvocation : BaseRequestHandleInvocation
    {
        private readonly IStubMessageBus stubMessageBus;

        public StubRequestHandleInvocation(IStubMessageBus stubMessageBus)
        {
            this.stubMessageBus = stubMessageBus;
        }

        public override void Proceed()
        {
            var expectation = stubMessageBus.GetExpectationFor(Request.GetType());

            if (expectation == null)
            {
                Response = (Response)Activator.CreateInstance(Request.GetResponseType());
                Response.CorrelationGuid = Request.CorrelationGuid;
            }
            else
            {
                Response = (Response)expectation.Execute(Request);
            }
        }
    }
}
