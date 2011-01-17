using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Colombo.TestSupport
{
    public class RequestHandlerExpectation<THandler> : BaseExpectation
        where THandler : IRequestHandler
    {
        public RequestHandlerExpectation(IStubMessageBus stubMessageBus) : base(stubMessageBus)
        {
            ExpectedNumCalled = 1;
        }

        internal override object Execute(object parameter)
        {
            ++NumCalled;
            var requestHandler = StubMessageBus.Kernel.Resolve<THandler>();
            return requestHandler.Handle((BaseRequest)parameter);
        }

        public RequestHandlerExpectation<THandler> Repeat(int times)
        {
            ExpectedNumCalled = times;
            return this;
        }

        public override void Verify()
        {
            if (ExpectedNumCalled != NumCalled)
                throw new ColomboExpectationException(string.Format("Expected request handler {0} to be invoked {1} time(s), actual: {2}", typeof(THandler), ExpectedNumCalled, NumCalled));
        }
    }
}
