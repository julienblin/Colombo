using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Colombo.Tests
{
    [TestFixture]
    public class SideEffectFreeRequestHandlerTest
    {
        static Guid CorrelationGuid = Guid.NewGuid();

        [Test]
        public void It_should_create_a_default_response_and_set_CorrelationGuid()
        {
            var requestHandler = new TestSideEffectFreeRequestHandler();
            var request = new TestSideEffectFreeRequest();
            request.CorrelationGuid = CorrelationGuid;

            requestHandler.Handle(request);
            Assert.That(() => requestHandler.HandleWasCalled,
                Is.True);
        }

        public class TestSideEffectFreeRequest : SideEffectFreeRequest<TestResponse>
        {
        }

        public class TestSideEffectFreeRequestHandler : SideEffectFreeRequestHandler<TestSideEffectFreeRequest, TestResponse>
        {
            public bool HandleWasCalled { get; set; }

            protected override void Handle()
            {
                HandleWasCalled = true;
                Assert.That(() => Response.CorrelationGuid,
                    Is.EqualTo(CorrelationGuid));

            }
        }

        public delegate void HandleDelegate();
    }
}
