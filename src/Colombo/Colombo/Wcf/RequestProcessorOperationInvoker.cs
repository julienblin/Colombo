using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Dispatcher;

namespace Colombo.Wcf
{
    public class RequestProcessorOperationInvoker : IOperationInvoker
    {
        private readonly Type requestType;

        public RequestProcessorOperationInvoker(Type requestType)
        {
            this.requestType = requestType;
        }

        public object[] AllocateInputs()
        {
            return new object[] { Activator.CreateInstance(requestType) };
        }

        public object Invoke(object instance, object[] inputs, out object[] outputs)
        {
            var request = (BaseRequest)inputs[0];
            var responses = WcfServices.ProcessLocally(new BaseRequest[] { request });
            outputs = new object[0];
            return responses[0];
        }

        public IAsyncResult InvokeBegin(object instance, object[] inputs, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        public object InvokeEnd(object instance, out object[] outputs, IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        public bool IsSynchronous
        {
            get { return true; }
        }
    }
}
