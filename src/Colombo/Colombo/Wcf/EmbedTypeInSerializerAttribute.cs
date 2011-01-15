using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Xml;

namespace Colombo.Wcf
{
    [AttributeUsage(AttributeTargets.Method)]
    public class EmbedTypeInSerializerAttribute : Attribute, IOperationBehavior
    {
        public void AddBindingParameters(OperationDescription operationDescription, BindingParameterCollection bindingParameters)
        {

        }

        public void ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
        {
            ForceNetDataContractSerializerOperationBehavior(operationDescription);
        }

        public void ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
        {
            ForceNetDataContractSerializerOperationBehavior(operationDescription);
        }

        public void Validate(OperationDescription operationDescription)
        {

        }

        private static void ForceNetDataContractSerializerOperationBehavior(OperationDescription operationDescription)
        {
            var dcsOperationBehavior = operationDescription.Behaviors.Find<DataContractSerializerOperationBehavior>();
            if (dcsOperationBehavior != null)
            {
                operationDescription.Behaviors.Remove(dcsOperationBehavior);
                operationDescription.Behaviors.Add(new NetDataContractSerializerOperationBehavior(operationDescription));
            }
        }
    }

    public class NetDataContractSerializerOperationBehavior : DataContractSerializerOperationBehavior
    {
        public NetDataContractSerializerOperationBehavior(OperationDescription operationDescription)
            : base(operationDescription)
        {
        }

        public override XmlObjectSerializer CreateSerializer(Type type, string name, string ns, IList<Type> knownTypes)
        {
            return new NetDataContractSerializer();
        }

        public override XmlObjectSerializer CreateSerializer(Type type, XmlDictionaryString name, XmlDictionaryString ns, IList<Type> knownTypes)
        {
            return new NetDataContractSerializer();
        }
    }
}
