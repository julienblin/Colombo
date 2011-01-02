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
        public void It_should_ensure_that_at_a_IColomboConfiguration_is_provided()
        {
            Assert.That(() => new WcfClientMessageProcessor(null),
                Throws.Exception.TypeOf<ArgumentNullException>()
                .With.Message.Contains("colomboConfiguration"));
        }

        [Test]
        public void It_should_rely_on_IColomboConfiguration_to_determine_if_can_send()
        {
            var mocks = new MockRepository();
            var request = mocks.Stub<Request<TestResponse>>();
            var config1 = mocks.StrictMock<IColomboConfiguration>();
            var config2 = mocks.StrictMock<IColomboConfiguration>();

            With.Mocks(mocks).Expecting(() =>
            {
                Expect.Call(config1.GetTargetAddressFor(request, WcfClientMessageProcessor.WcfEndPointType)).Return(null);
                Expect.Call(config2.GetTargetAddressFor(request, WcfClientMessageProcessor.WcfEndPointType)).Return(@"http://localhost/Colombo.svc");
            }).Verify(() =>
            {
                var processor = new WcfClientMessageProcessor(config1);
                Assert.That(() => processor.CanSend(request),
                    Is.False);

                processor = new WcfClientMessageProcessor(config2);
                Assert.That(() => processor.CanSend(request),
                    Is.True);
            });
        }

        [Test]
        public void It_should_create_a_ClientBase_from_IColomboConfiguration()
        {
            var mocks = new MockRepository();
            var requestHttp = mocks.Stub<Request<TestResponse>>();
            var requestNetTcp = mocks.Stub<Request<TestResponse>>();
            var config = mocks.StrictMock<IColomboConfiguration>();

            With.Mocks(mocks).Expecting(() =>
            {
                Expect.Call(config.GetTargetAddressFor(requestHttp, WcfClientMessageProcessor.WcfEndPointType)).Return(@"http://localhost/Colombo.svc");
                Expect.Call(config.GetTargetAddressFor(requestNetTcp, WcfClientMessageProcessor.WcfEndPointType)).Return(@"net.tcp://localhost/Colombo");
            }).Verify(() =>
            {
                var processor = new WcfClientMessageProcessor(config);
                var clientBase = processor.CreateClientBase(requestHttp);
                Assert.That(() => clientBase.Endpoint.Binding,
                    Is.TypeOf<BasicHttpBinding>());
                Assert.That(() => clientBase.Endpoint.Address.Uri,
                    Is.EqualTo(new Uri(@"http://localhost/Colombo.svc")));

                clientBase = processor.CreateClientBase(requestNetTcp);
                Assert.That(() => clientBase.Endpoint.Binding,
                    Is.TypeOf<NetTcpBinding>());
                Assert.That(() => clientBase.Endpoint.Address.Uri,
                    Is.EqualTo(new Uri(@"net.tcp://localhost/Colombo")));
            });
        }

        [Test]
        public void It_should_throw_an_exception_when_creating_ClientBase_with_malformed_uri()
        {
            var mocks = new MockRepository();
            var request = mocks.Stub<Request<TestResponse>>();
            var config = mocks.StrictMock<IColomboConfiguration>();

            With.Mocks(mocks).Expecting(() =>
            {
                Expect.Call(config.GetTargetAddressFor(request, WcfClientMessageProcessor.WcfEndPointType)).Return(@"abcdef");
            }).Verify(() =>
            {
                var processor = new WcfClientMessageProcessor(config);
                Assert.That(() => processor.CreateClientBase(request),
                    Throws.Exception.TypeOf<ColomboException>()
                    .With.Message.Contains("Malformed Uri")
                    .With.Message.Contains("abcdef"));
            });
        }

        [Test]
        public void It_should_throw_an_exception_when_creating_ClientBase_with_unrecognized_uri_scheme()
        {
            var mocks = new MockRepository();
            var request = mocks.Stub<Request<TestResponse>>();
            var config = mocks.StrictMock<IColomboConfiguration>();

            With.Mocks(mocks).Expecting(() =>
            {
                Expect.Call(config.GetTargetAddressFor(request, WcfClientMessageProcessor.WcfEndPointType)).Return(@"net.myprotocol://localhost/Colombo");
            }).Verify(() =>
            {
                var processor = new WcfClientMessageProcessor(config);
                Assert.That(() => processor.CreateClientBase(request),
                    Throws.Exception.TypeOf<ColomboException>()
                    .With.Message.Contains("Unrecognized Uri")
                    .With.Message.Contains("net.myprotocol"));
            });
        }
    }
}
