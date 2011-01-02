using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Colombo.Wcf;
using Rhino.Mocks;

namespace Colombo.Tests.Wcf
{
    [TestFixture]
    public class WcfClientMessageProcessorTest
    {
        [Test]
        public void It_Should_Ensure_That_At_A_IColomboConfiguration_Is_Provided()
        {
            Assert.That(() => new WcfClientMessageProcessor(null),
                Throws.Exception.TypeOf<ArgumentNullException>()
                .With.Message.Contains("colomboConfiguration"));
        }

        [Test]
        public void It_Should_Rely_On_IColomboConfiguration_To_Determine_If_Can_Send()
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
    }
}
