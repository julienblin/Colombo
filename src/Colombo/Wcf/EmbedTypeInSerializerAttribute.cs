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
