using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Colombo.Wcf;
using Rhino.Mocks;
using System.ServiceModel;

namespace Colombo.Tests.Wcf
{
    [TestFixture]
    public class WcfClientMessageProcessorTest
    {
        [Test]
        public void It_should_check_WCF_client_configuration_to_determine_if_it_CanSend()
        {
            var mocks = new MockRepository();

            var processor = new WcfClientMessageProcessor();
            var requestInThisAssembly = new TestRequest();
            var requestInDynamicAssembly = mocks.Stub<Request<TestResponse>>();

            Assert.That(() => processor.CanSend(requestInThisAssembly),
                Is.True);
            Assert.That(() => processor.CanSend(requestInDynamicAssembly),
                Is.False);
        }

        [Test]
        public void It_should_create_a_ClientBase_from_configuration()
        {
            var processor = new WcfClientMessageProcessor();
            var requestInThisAssembly = new TestRequest();

            var clientBase = processor.CreateClientBase(requestInThisAssembly);

            Assert.That(() => clientBase.Endpoint.Address.Uri,
                Is.EqualTo(new Uri(@"http://localhost/Colombo.svc")));
        }

        public class TestRequest : Request<TestResponse>
        {
        }
    }
}
