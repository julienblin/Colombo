using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Colombo.TestSupport
{
    public class MessageBusSendExpectation<TRequest, TResponse> : BaseExpectation
        where TRequest : BaseRequest, new()
        where TResponse : Response, new()
    {
        public MessageBusSendExpectation(IStubMessageBus stubMessageBus)
            : base(stubMessageBus)
        {
            ExpectedNumCalled = 1;
        }

        private Action<TRequest, TResponse> recordedAction;

        public MessageBusSendExpectation<TRequest, TResponse> Reply(Action<TRequest, TResponse> action)
        {
            recordedAction = action;
            return this;
        }

        public MessageBusSendExpectation<TRequest, TResponse> Repeat(int times)
        {
            ExpectedNumCalled = times;
            return this;
        }

        internal override object Execute(object parameter)
        {
            ++NumCalled;
            var request = (TRequest)parameter;
            if (recordedAction != null)
            {
                var response = new TResponse { CorrelationGuid = request.CorrelationGuid };
                recordedAction.Invoke(request, response);
                return response;
            }
            return null;
        }

        public override void Verify()
        {
            if (ExpectedNumCalled != NumCalled)
                throw new ColomboExpectationException(string.Format("Expected {0} to be sent {1} time(s), actual: {2}", typeof(TRequest), ExpectedNumCalled, NumCalled));
        }
    }
}
