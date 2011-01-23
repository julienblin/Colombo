using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;
using System.ServiceModel.Dispatcher;
using System.Xml;

namespace Colombo.Wcf
{
    /// <summary>
    /// <see cref="IOperationBehavior"/> that forces the usage of the <see cref="NetDataContractSerializer"/>.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class EmbedTypeInSerializerAttribute : Attribute, IOperationBehavior
    {
        /// <summary>
        /// Do nothing.
        /// </summary>
        public void AddBindingParameters(OperationDescription operationDescription, BindingParameterCollection bindingParameters)
        {

        }

        /// <summary>
        /// Attach a <see cref="NetDataContractSerializerOperationBehavior"/> and remove <see cref="DataContractSerializerOperationBehavior"/>.
        /// </summary>
        public void ApplyClientBehavior(OperationDescription operationDescription, ClientOperation clientOperation)
        {
            ForceNetDataContractSerializerOperationBehavior(operationDescription);
        }

        /// <summary>
        /// Attach a <see cref="NetDataContractSerializerOperationBehavior"/> and remove <see cref="DataContractSerializerOperationBehavior"/>.
        /// </summary>
        public void ApplyDispatchBehavior(OperationDescription operationDescription, DispatchOperation dispatchOperation)
        {
            ForceNetDataContractSerializerOperationBehavior(operationDescription);
        }

        /// <summary>
        /// Do nothing.
        /// </summary>
        public void Validate(OperationDescription operationDescription)
        {

        }

        private static void ForceNetDataContractSerializerOperationBehavior(OperationDescription operationDescription)
        {
            var dcsOperationBehavior = operationDescription.Behaviors.Find<DataContractSerializerOperationBehavior>();
            if (dcsOperationBehavior == null) return;

            operationDescription.Behaviors.Remove(dcsOperationBehavior);
            operationDescription.Behaviors.Add(new NetDataContractSerializerOperationBehavior(operationDescription));
        }
    }

    /// <summary>
    /// Forces the usage of the <see cref="NetDataContractSerializer"/>.
    /// </summary>
    public class NetDataContractSerializerOperationBehavior : DataContractSerializerOperationBehavior
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public NetDataContractSerializerOperationBehavior(OperationDescription operationDescription)
            : base(operationDescription)
        {
        }

        /// <summary>
        /// Returns a <see cref="NetDataContractSerializer"/>.
        /// </summary>
        public override XmlObjectSerializer CreateSerializer(Type type, string name, string ns, IList<Type> knownTypes)
        {
            return new NetDataContractSerializer();
        }

        /// <summary>
        /// Returns a <see cref="NetDataContractSerializer"/>.
        /// </summary>
        public override XmlObjectSerializer CreateSerializer(Type type, XmlDictionaryString name, XmlDictionaryString ns, IList<Type> knownTypes)
        {
            return new NetDataContractSerializer();
        }
    }
}
