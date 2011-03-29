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
using System.IO;
using System.Runtime.Serialization;
using Castle.DynamicProxy;
using Colombo.Impl.Async;
using Colombo.Impl.RequestHandle;

namespace Colombo.TestSupport
{
    internal class StubRequestHandleInvocation : BaseRequestHandleInvocation
    {
        private readonly IStubMessageBus stubMessageBus;

        public StubRequestHandleInvocation(IStubMessageBus stubMessageBus)
        {
            this.stubMessageBus = stubMessageBus;
        }

        public override void Proceed()
        {
            var expectation = stubMessageBus.GetExpectationFor(Request.GetType());

            if (expectation == null)
            {
                var response = (Response)Activator.CreateInstance(Request.GetResponseType());
                response.CorrelationGuid = Request.CorrelationGuid;
                Response = GetSerializedVersion(response);
            }
            else
            {
                Response = GetSerializedVersion(expectation.Execute(Request));
            }

            if(Request.IsSideEffectFree)
            {
                try
                {
                    var options = new ProxyGenerationOptions(new NonVirtualCheckProxyGenerationHook());
                    var proxyGen = new ProxyGenerator();
                    proxyGen.CreateClassProxy(Request.GetResponseType(), options);
                }
                catch (Exception ex)
                {
                    throw new ColomboTestSupportException(string.Format("Response {0} cannot be proxied, probably because one or several of its members are not virtual.", Request.GetResponseType()), ex);
                }
            }
        }

        private static Response GetSerializedVersion(object response)
        {
            try
            {
                using (var stream = new MemoryStream())
                {
                    var serializer = new NetDataContractSerializer();
                    serializer.WriteObject(stream, response);
                    stream.Position = 0;
                    return (Response)serializer.ReadObject(stream);
                }
            }
            catch (Exception ex)
            {
                throw new ColomboSerializationException(string.Format("{0} could not be serialized.", response), ex);
            }
        }
    }
}
