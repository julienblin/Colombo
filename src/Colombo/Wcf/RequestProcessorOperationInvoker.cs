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
