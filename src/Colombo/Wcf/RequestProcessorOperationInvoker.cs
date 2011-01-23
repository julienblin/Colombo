using System;
using System.ServiceModel.Dispatcher;

namespace Colombo.Wcf
{
    /// <summary>
    /// <see cref="IOperationInvoker"/> that redirect calls to dynamic methods added by <see cref="AddOperationsForRequestHandlersAttribute"/>
    /// to requests handlers.
    /// </summary>
    public class RequestProcessorOperationInvoker : IOperationInvoker
    {
        private readonly Type requestType;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="requestType">Type of the request.</param>
        public RequestProcessorOperationInvoker(Type requestType)
        {
            this.requestType = requestType;
        }

        /// <summary>
        /// Returns an <see cref="T:System.Array"/> of parameter objects.
        /// </summary>
        /// <returns>
        /// The parameters that are to be used as arguments to the operation.
        /// </returns>
        public object[] AllocateInputs()
        {
            return new[] { Activator.CreateInstance(requestType) };
        }

        /// <summary>
        /// Returns an object and a set of output objects from an instance and set of input objects.  
        /// </summary>
        /// <returns>
        /// The return value.
        /// </returns>
        /// <param name="instance">The object to be invoked.</param><param name="inputs">The inputs to the method.</param><param name="outputs">The outputs from the method.</param>
        public object Invoke(object instance, object[] inputs, out object[] outputs)
        {
            var request = (BaseRequest)inputs[0];
            var responses = WcfServices.ProcessLocally(new[] { request });
            outputs = new object[0];
            return responses[0];
        }

        /// <summary>
        /// Not Implemented.
        /// </summary>
        public IAsyncResult InvokeBegin(object instance, object[] inputs, AsyncCallback callback, object state)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Not Implemented.
        /// </summary>
        public object InvokeEnd(object instance, out object[] outputs, IAsyncResult result)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// True.
        /// </summary>
        public bool IsSynchronous
        {
            get { return true; }
        }
    }
}
