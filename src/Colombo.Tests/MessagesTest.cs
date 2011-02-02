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

using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Castle.DynamicProxy;
using Colombo.Impl.Async;
using NUnit.Framework;

namespace Colombo.Tests
{
    [TestFixture]
    public class MessagesTest : BaseTest
    {
        [Test]
        public void Responses_should_be_serializables_using_the_DataContractSerializer()
        {
            using (var stream = new MemoryStream())
            {
                var serializer = new DataContractSerializer(typeof(TestResponse));
                var reference = new TestResponse();
                serializer.WriteObject(stream, reference);
                stream.Position = 0;
                var deserialized = (TestResponse)serializer.ReadObject(stream);
                Assert.AreNotSame(deserialized, reference);
                Assert.AreEqual(deserialized.CorrelationGuid, reference.CorrelationGuid);
                Assert.AreEqual(deserialized.UtcTimestamp, reference.UtcTimestamp);
            }
        }

        [Test]
        public void ValidatedResponses_should_be_serializables_using_the_DataContractSerializer()
        {
            using (var stream = new MemoryStream())
            {
                var serializer = new DataContractSerializer(typeof(TestValidatedResponse));
                var reference = new TestValidatedResponse();
                reference.ValidationResults.Add(new ValidationResult("TestErrorMessage", new string[] { "Member1", "Member2" }));
                serializer.WriteObject(stream, reference);
                stream.Position = 0;
                var deserialized = (TestValidatedResponse)serializer.ReadObject(stream);
                Assert.AreNotSame(deserialized, reference);
                Assert.AreEqual(deserialized.CorrelationGuid, reference.CorrelationGuid);
                Assert.AreEqual(deserialized.UtcTimestamp, reference.UtcTimestamp);
                Assert.AreEqual(deserialized.ValidationResults[0].ErrorMessage, "TestErrorMessage");
                Assert.AreEqual(deserialized.ValidationResults[0].MemberNames.ToArray()[0], "Member1");
                Assert.AreEqual(deserialized.ValidationResults[0].MemberNames.ToArray()[1], "Member2");
            }
        }

        [Test]
        public void Responses_should_be_usable_with_proxy()
        {
            var options = new ProxyGenerationOptions(new NonVirtualCheckProxyGenerationHook());
            var proxyGen = new ProxyGenerator();
            Assert.That(() => proxyGen.CreateClassProxy<Response>(options),
                Is.Not.Null);
        }

        [Test]
        public void ValidatedResponses_should_be_usable_with_proxy()
        {
            var options = new ProxyGenerationOptions(new NonVirtualCheckProxyGenerationHook());
            var proxyGen = new ProxyGenerator();
            Assert.That(() => proxyGen.CreateClassProxy<ValidatedResponse>(options),
                Is.Not.Null);
        }

        [Test]
        public void Requests_should_be_serializables_using_the_DataContractSerializer()
        {
            using (var stream = new MemoryStream())
            {
                var serializer = new DataContractSerializer(typeof(TestRequest));
                var reference = new TestRequest();
                reference.Context[@"SomeKey"] = "SomeValue";
                serializer.WriteObject(stream, reference);
                stream.Position = 0;
                var deserialized = (TestRequest)serializer.ReadObject(stream);
                Assert.AreNotSame(deserialized, reference);
                Assert.AreEqual(deserialized.CorrelationGuid, reference.CorrelationGuid);
                Assert.AreEqual(deserialized.UtcTimestamp, reference.UtcTimestamp);
                Assert.AreEqual(deserialized.Context[@"SomeKey"], reference.Context[@"SomeKey"]);
            }
        }

        [Test]
        public void SideEffectFreeRequests_should_be_serializables_using_the_DataContractSerializer()
        {
            using (var stream = new MemoryStream())
            {
                var serializer = new DataContractSerializer(typeof(TestSideEffectFreeRequest));
                var reference = new TestSideEffectFreeRequest();
                reference.Context[@"SomeKey"] = "SomeValue";
                serializer.WriteObject(stream, reference);
                stream.Position = 0;
                var deserialized = (TestSideEffectFreeRequest)serializer.ReadObject(stream);
                Assert.AreNotSame(deserialized, reference);
                Assert.AreEqual(deserialized.CorrelationGuid, reference.CorrelationGuid);
                Assert.AreEqual(deserialized.UtcTimestamp, reference.UtcTimestamp);
                Assert.AreEqual(deserialized.Context[@"SomeKey"], reference.Context[@"SomeKey"]);
            }
        }

        [Test]
        public void BaseRequest_should_default_ToString_to_Type_CorrelationGuid_TimeStamp_and_Context_without_Meta()
        {
            var request = new TestRequest();
            request.Context["key1"] = "value1";
            request.Context[MetaContextKeys.MetaPrefix + "key2"] = "value2";

            var result = request.ToString();

            Assert.That(result, Contains.Substring(request.GetType().Name));
            Assert.That(result, Contains.Substring(request.CorrelationGuid.ToString()));
            Assert.That(result, Contains.Substring(request.UtcTimestamp.ToString("yyyy-MM-dd-HH:mm:ss")));
            Assert.That(result, Contains.Substring("key1=value1"));
            Assert.That(result, !Contains.Substring(MetaContextKeys.MetaPrefix + "key2=value2"));
        }

        public class TestRequest : Request<TestResponse> { }

        public class TestSideEffectFreeRequest : SideEffectFreeRequest<TestResponse> { }

        public class TestValidatedResponse : ValidatedResponse { }
    }
}
