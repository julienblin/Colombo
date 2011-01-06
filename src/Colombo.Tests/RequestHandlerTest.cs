using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
namespace Colombo.Tests
{
    [TestFixture]
    public class RequestHandlerTest
    {
        static Guid CorrelationGuid = Guid.NewGuid();

        [Test]
        public void It_should_create_a_default_response_and_set_CorrelationGuid()
        {
            var requestHandler = new TestRequestHandler();
            var request = new TestRequest();
            request.CorrelationGuid = CorrelationGuid;

            requestHandler.Handle(request);
            Assert.That(() => requestHandler.HandleWasCalled,
                Is.True);
        }

        public class TestRequest : Request<TestResponse>
        {
        }

        public class TestRequestHandler : RequestHandler<TestRequest, TestResponse>
        {
            public bool HandleWasCalled { get; set; }

            public override void Handle()
            {
                HandleWasCalled = true;
                Assert.That(() => Response.CorrelationGuid,
                    Is.EqualTo(CorrelationGuid));

            }
        }

        public delegate void HandleDelegate();
    }
}
