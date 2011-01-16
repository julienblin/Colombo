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
        public void Notifications_should_be_serializables_using_the_DataContractSerializer()
        {
            using (var stream = new MemoryStream())
            {
                var serializer = new DataContractSerializer(typeof(TestNotification));
                var reference = new TestNotification();
                reference.Context[@"SomeKey"] = "SomeValue";
                serializer.WriteObject(stream, reference);
                stream.Position = 0;
                var deserialized = (TestNotification)serializer.ReadObject(stream);
                Assert.AreNotSame(deserialized, reference);
                Assert.AreEqual(deserialized.CorrelationGuid, reference.CorrelationGuid);
                Assert.AreEqual(deserialized.UtcTimestamp, reference.UtcTimestamp);
                Assert.AreEqual(deserialized.Context[@"SomeKey"], reference.Context[@"SomeKey"]);
            }
        }

        public class TestRequest : Request<TestResponse> { }

        public class TestSideEffectFreeRequest : SideEffectFreeRequest<TestResponse> { }

        public class TestValidatedResponse : ValidatedResponse { }

        public class TestNotification : Notification { }
    }
}
