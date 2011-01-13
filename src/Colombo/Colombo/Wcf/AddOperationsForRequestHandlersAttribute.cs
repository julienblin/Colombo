using System;
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
        private Dictionary<string, Pair<Type, Type>> operationTypesByName = new Dictionary<string, Pair<Type, Type>>();

        public void AddBindingParameters(ContractDescription contractDescription, ServiceEndpoint endpoint, BindingParameterCollection bindingParameters)
        {
            // Removing the dummy operation. At this point, WCF has loaded everything it needs and doesn't need the dummy operation anymore.
            contractDescription.Operations.Remove(contractDescription.Operations.Where(x => x.Name.Equals("DummyOperationForWCF")).First());

            var allHandlers = WcfService.Kernel.ResolveAll<IRequestHandler>();

            foreach (var requestHandler in allHandlers)
            {
                var requestType = requestHandler.GetRequestType();
                var responseType = requestHandler.GetResponseType();

                var baseOperationName = requestHandler.GetType().Name.Replace("RequestHandler", "").Replace("Request", "");
                operationTypesByName[baseOperationName] = new Pair<Type, Type>(requestType, responseType);

                var operationDescription = new OperationDescription(baseOperationName, contractDescription);

                MessageDescription inputMessage = new MessageDescription("http://Colombo/WcfSoapService/" + baseOperationName, MessageDirection.Input);
                inputMessage.Body.WrapperName = baseOperationName;
                inputMessage.Body.WrapperNamespace = "http://Colombo";
                MessagePartDescription messagePartDescription = new MessagePartDescription(requestType.Name, "http://Colombo");
                messagePartDescription.Type = requestType;
                inputMessage.Body.Parts.Add(messagePartDescription);
                operationDescription.Messages.Add(inputMessage);

                MessageDescription outputMessage = new MessageDescription("http://Colombo/WcfSoapService/" + baseOperationName + "Response", MessageDirection.Output);
                outputMessage.Body.ReturnValue = new MessagePartDescription(baseOperationName + "Result", "http://Colombo");
                outputMessage.Body.ReturnValue.Type = responseType;
                outputMessage.Body.WrapperName = baseOperationName + "Response";
                outputMessage.Body.WrapperNamespace = "http://Colombo";
                operationDescription.Messages.Add(outputMessage);

                OperationBehaviorAttribute operationBehaviourAttribute = new OperationBehaviorAttribute();
                operationDescription.Behaviors.Add(operationBehaviourAttribute);

                DataContractSerializerOperationBehavior d =
                    new DataContractSerializerOperationBehavior(operationDescription);
                operationDescription.Behaviors.Add(d);

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
                if (!operation.Name.Equals("Ping", StringComparison.InvariantCultureIgnoreCase))
                {
                    var operationTypes = operationTypesByName[operation.Name];
                    operation.Invoker = new RequestProcessorOperationInvoker(operationTypes.First, operationTypes.Second);
                }

            }
        }

        public void Validate(ContractDescription contractDescription, ServiceEndpoint endpoint)
        {

        }
    }
}
