using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Colombo.Samples.Hosted.Handlers;
using Colombo.Samples.Messages;
using NUnit.Framework;

namespace Hosted.Tests
{
    [TestFixture]
    public class HelloWorldHandlerTest : BaseTest
    {
        [Test]
        public void It_should_respond_using_the_request_Name_and_notify()
        {
            var request = new HelloWorldRequest() { Name = "TheName" };

            StubMessageBus.TestHandler<HelloWorldHandler>();
            StubMessageBus
                .ExpectNotify<HelloWorldNotification>()
                .Assert(n => n.Name.Equals(request.Name));

            var response = MessageBus.Send(request);

            Assert.That(response.Message.Contains("TheName"));
            StubMessageBus.Verify();
        }

        [Test]
        public void It_should_reject_Requests_without_Name_and_not_notify()
        {
            StubMessageBus.TestHandler<HelloWorldHandler>().ShouldBeInterceptedBeforeHandling();

            var response = MessageBus.Send(new HelloWorldRequest());

            Assert.IsFalse(response.IsValid());
            StubMessageBus.Verify();
        }
    }
}
