using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using System.Runtime.Serialization;
using System.IO;

namespace Colombo.Tests
{
    [TestFixture]
    public class MessagesTest
    {
        [Test]
        public void Responses_should_be_serializables_using_the_DataContractSerializer()
        {
            using (var stream = new MemoryStream())
            {
                var serializer = new DataContractSerializer(typeof(TestResponse));
                var reference= new TestResponse();
                serializer.WriteObject(stream, reference);
                stream.Position = 0;
                var deserialized = (TestResponse)serializer.ReadObject(stream);
                Assert.AreNotSame(deserialized, reference);
                Assert.AreEqual(deserialized.CorrelationGuid, reference.CorrelationGuid);
                Assert.AreEqual(deserialized.Timestamp, reference.Timestamp);
            }
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
                Assert.AreEqual(deserialized.Timestamp, reference.Timestamp);
                Assert.AreEqual(deserialized.Context[@"SomeKey"], reference.Context[@"SomeKey"]);
            }
        }

        public class TestResponse : Response { }
        public class TestRequest : Request<TestResponse> { }
    }
}
