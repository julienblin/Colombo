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
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;

namespace Colombo.Wcf
{
    /// <summary>
    /// <see cref="IContractBehavior"/> that dynamically adds registered request handlers operations as methods on a <see cref="ServiceContractAttribute"/>
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface)]
    public class AddOperationsForRequestHandlersAttribute : Attribute, IContractBehavior
    {
        private readonly Dictionary<string, Type> operationRequestTypeByName = new Dictionary<string, Type>();

        /// <summary>
        /// Adds all the dynamic operation to the <paramref name="contractDescription"/>
        /// </summary>
        public void AddBindingParameters(ContractDescription contractDescription, ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
            // Removing the dummy operation. At this point, WCF has loaded everything it needs and doesn't need the dummy operation anymore.
            if(contractDescription.Operations.Count > 0)
                contractDescription.Operations.Remove(contractDescription.Operations.Where(x => x.Name.Equals("DummyOperationForWCF")).First());

            if (WcfServices.Kernel == null)
                throw new ColomboException("WcfServices.Kernel is null. You must register a IKernel through the WcfServices.Kernel static attribute.");

            var allHandlers = WcfServices.Kernel.ResolveAll<IRequestHandler>();

            foreach (var requestHandler in allHandlers)
            {
                var requestType = requestHandler.GetRequestType();
                var responseType = requestHandler.GetResponseType();

                var baseOperationName = requestType.Name.Replace("Request", "");
                operationRequestTypeByName[baseOperationName] = requestType;

                var operationDescription = new OperationDescription(baseOperationName, contractDescription);

                const string messageDescriptionBase = WcfServices.Namespace + "/SoapService/";
                var inputMessage = new MessageDescription(messageDescriptionBase + baseOperationName, MessageDirection.Input);
                inputMessage.Body.WrapperName = baseOperationName;
                inputMessage.Body.WrapperNamespace = WcfServices.Namespace;
                var messagePartDescription = new MessagePartDescription(requestType.Name, WcfServices.Namespace)
                                                 {Type = requestType};
                inputMessage.Body.Parts.Add(messagePartDescription);
                operationDescription.Messages.Add(inputMessage);

                var outputMessage = new MessageDescription(messageDescriptionBase + baseOperationName + "Response", MessageDirection.Output);
                outputMessage.Body.ReturnValue = new MessagePartDescription(baseOperationName + "Result", WcfServices.Namespace)
                                                     {Type = responseType};
                outputMessage.Body.WrapperName = baseOperationName + "Response";
                outputMessage.Body.WrapperNamespace = WcfServices.Namespace;
                operationDescription.Messages.Add(outputMessage);

                operationDescription.Behaviors.Add(new OperationBehaviorAttribute());
                operationDescription.Behaviors.Add(new DataContractSerializerOperationBehavior(operationDescription));

                contractDescription.Operations.Add(operationDescription);
            }
        }

        /// <summary>
        /// Do nothing
        /// </summary>
        public void ApplyClientBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {

        }

        /// <summary>
        /// Binds each operation to the <see cref="RequestProcessorOperationInvoker"/> invoker.
        /// </summary>
        public void ApplyDispatchBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, DispatchRuntime dispatchRuntime)
        {
            foreach (var operation in dispatchRuntime.Operations)
            {
                var requestType = operationRequestTypeByName[operation.Name];
                operation.Invoker = new RequestProcessorOperationInvoker(requestType);
            }
        }

        /// <summary>
        /// Do nothing
        /// </summary>
        public void Validate(ContractDescription contractDescription, ServiceEndpoint endpoint)
        {

        }
    }
}
