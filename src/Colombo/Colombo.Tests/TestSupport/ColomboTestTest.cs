using Colombo.TestSupport;
using NUnit.Framework;
using System.ComponentModel.DataAnnotations;

namespace Colombo.Tests.TestSupport
{
    [TestFixture]
    public class ColomboTestTest
    {
        [Test]
        public void It_should_reject_requests_that_cannot_be_created_using_Activator()
        {
            Assert.That(() => ColomboTest.AssertThat.RequestIsConform<RequestWithoutDefaultConstructor>(),
                Throws.Exception.TypeOf<ColomboTestException>()
                .With.Message.Contains("default constructor"));
        }

        [Test]
        public void It_should_reject_requests_that_are_not_serializable_with_DataContractSerializer()
        {
            Assert.That(() => ColomboTest.AssertThat.RequestIsConform<RequestNotSerializable>(),
                Throws.Exception.TypeOf<ColomboTestException>()
                .With.Message.Contains("DataContractSerializer"));
        }

        [Test]
        public void It_should_reject_notifications_that_cannot_be_created_using_Activator()
        {
            Assert.That(() => ColomboTest.AssertThat.NotificationIsConform<NotificationWithoutDefaultConstructor>(),
                Throws.Exception.TypeOf<ColomboTestException>()
                .With.Message.Contains("default constructor"));
        }

        [Test]
        public void It_should_reject_notifications_that_are_not_serializable_with_DataContractSerializer()
        {
            Assert.That(() => ColomboTest.AssertThat.NotificationIsConform<NotificationNotSerializable>(),
                Throws.Exception.TypeOf<ColomboTestException>()
                .With.Message.Contains("DataContractSerializer"));
        }

        [Test]
        public void It_should_reject_responses_that_cannot_be_created_using_Activator()
        {
            Assert.That(() => ColomboTest.AssertThat.ResponseIsConform<ResponseWithoutDefaultConstructor>(),
                Throws.Exception.TypeOf<ColomboTestException>()
                .With.Message.Contains("default constructor"));
        }

        [Test]
        public void It_should_reject_responses_that_are_not_serializable_with_DataContractSerializer()
        {
            Assert.That(() => ColomboTest.AssertThat.ResponseIsConform<ResponseNotSerializable>(),
                Throws.Exception.TypeOf<ColomboTestException>()
                .With.Message.Contains("DataContractSerializer"));
        }

        [Test]
        public void It_should_reject_responses_when_a_member_is_not_virtual()
        {
            Assert.That(() => ColomboTest.AssertThat.ResponseIsConform<ResponseWithNonVirtualMember>(),
                Throws.Exception.TypeOf<ColomboTestException>()
                .With.Message.Contains("virtual"));
        }

        public class RequestWithoutDefaultConstructor : Request<TestResponse>
        {
            public RequestWithoutDefaultConstructor(string name)
            {
            }
        }

        public class RequestNotSerializable : Request<TestResponse>
        {
            public ValidationResult ValidationResult { get; set; }
        }

        public class NotificationWithoutDefaultConstructor : Notification
        {
            public NotificationWithoutDefaultConstructor(string name)
            {
            }
        }

        public class NotificationNotSerializable : Notification
        {
            public ValidationResult ValidationResult { get; set; }
        }

        public class ResponseWithoutDefaultConstructor : Response
        {
            public ResponseWithoutDefaultConstructor(string name)
            {
                    
            }
        }

        public class ResponseNotSerializable : Response
        {
            public virtual ValidationResult ValidationResult { get; set; }
        }

        public class ResponseWithNonVirtualMember : Response
        {
            public string Name { get; set; }
        }
    }
}
