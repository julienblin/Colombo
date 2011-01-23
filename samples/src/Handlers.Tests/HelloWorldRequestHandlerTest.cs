using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Colombo.Samples.Messages;
using NUnit.Framework;

namespace Colombo.Samples.Handlers.Tests
{
    [TestFixture]
    public class HelloWorldRequestHandlerTest : BaseHandlerTest
    {
        [Test]
        public void It_should_respond_with_the_name()
        {
            StubMessageBus.TestHandler<HelloWorldRequestHandler>();
            var request = new HelloWorldRequest { Name = "Foo" };

            var response = MessageBus.Send(request);

            Assert.That(() => response.Message, Contains.Substring(request.Name));
        }
    }
}
