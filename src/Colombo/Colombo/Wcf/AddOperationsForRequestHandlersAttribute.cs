﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel.Description;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Reflection;
using System.ServiceModel;
using Castle.Core;

namespace Colombo.Wcf
{
    [AttributeUsage(AttributeTargets.Interface)]
    public class AddOperationsForRequestHandlersAttribute : Attribute, IContractBehavior
    {
        private Dictionary<string, Type> operationRequestTypeByName = new Dictionary<string, Type>();

        public void AddBindingParameters(ContractDescription contractDescription, ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
            // Removing the dummy operation. At this point, WCF has loaded everything it needs and doesn't need the dummy operation anymore.
            contractDescription.Operations.Remove(contractDescription.Operations.Where(x => x.Name.Equals("DummyOperationForWCF")).First());

            var allHandlers = WcfServices.Kernel.ResolveAll<IRequestHandler>();

            foreach (var requestHandler in allHandlers)
            {
                var requestType = requestHandler.GetRequestType();
                var responseType = requestHandler.GetResponseType();

                var baseOperationName = requestType.Name.Replace("Request", "");
                operationRequestTypeByName[baseOperationName] = requestType;

                var operationDescription = new OperationDescription(baseOperationName, contractDescription);

                var messageDescriptionBase = WcfServices.Namespace + "/WcfSoapService/";
                var inputMessage = new MessageDescription(messageDescriptionBase + baseOperationName, MessageDirection.Input);
                inputMessage.Body.WrapperName = baseOperationName;
                inputMessage.Body.WrapperNamespace = WcfServices.Namespace;
                var messagePartDescription = new MessagePartDescription(requestType.Name, WcfServices.Namespace);
                messagePartDescription.Type = requestType;
                inputMessage.Body.Parts.Add(messagePartDescription);
                operationDescription.Messages.Add(inputMessage);

                var outputMessage = new MessageDescription(messageDescriptionBase + baseOperationName + "Response", MessageDirection.Output);
                outputMessage.Body.ReturnValue = new MessagePartDescription(baseOperationName + "Result", WcfServices.Namespace);
                outputMessage.Body.ReturnValue.Type = responseType;
                outputMessage.Body.WrapperName = baseOperationName + "Response";
                outputMessage.Body.WrapperNamespace = WcfServices.Namespace;
                operationDescription.Messages.Add(outputMessage);

                operationDescription.Behaviors.Add(new OperationBehaviorAttribute());
                operationDescription.Behaviors.Add(new DataContractSerializerOperationBehavior(operationDescription));

                contractDescription.Operations.Add(operationDescription);
            }
        }

        public void ApplyClientBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, ClientRuntime clientRuntime)
        {

        }

        public void ApplyDispatchBehavior(ContractDescription contractDescription, ServiceEndpoint endpoint, DispatchRuntime dispatchRuntime)
        {
            foreach (var operation in dispatchRuntime.Operations)
            {
                var requestType = operationRequestTypeByName[operation.Name];
                operation.Invoker = new RequestProcessorOperationInvoker(requestType);
            }
        }

        public void Validate(ContractDescription contractDescription, ServiceEndpoint endpoint)
        {

        }
    }
}