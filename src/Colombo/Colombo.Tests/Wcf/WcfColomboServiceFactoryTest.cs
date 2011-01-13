using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Colombo.Wcf;
using System.ServiceModel;

namespace Colombo.Tests.Wcf
{
    [TestFixture]
    public class WcfColomboServiceFactoryTest : BaseTest
    {
        [Test]
        public void It_should_check_WCF_client_configuration_to_determine_if_it_can_create_Channel()
        {
            var factory = new WcfColomboServiceFactory();
            Assert.That(() => factory.CanCreateChannelForRequestGroup("Colombo.Tests"),
                Is.True);
            Assert.That(() => factory.CanCreateChannelForRequestGroup("AnotherGroupName"),
                Is.False);
        }

        [Test]
        public void It_should_return_address_from_WCF_configuration()
        {
            var factory = new WcfColomboServiceFactory();
            Assert.That(() => factory.GetAddressForRequestGroup("Colombo.Tests"),
                Is.EqualTo(@"http://localhost/Colombo.svc"));
            Assert.That(() => factory.GetAddressForRequestGroup("AnotherAssembly"),
                Is.EqualTo(@"http://somewhereelse/Colombo.svc"));
            Assert.That(() => factory.GetAddressForRequestGroup("SomethingElse"),
                Is.Null);
        }

        [Test]
        public void It_should_create_a_Channel_from_configuration()
        {
            var factory = new WcfColomboServiceFactory();

            var wcfService = factory.CreateChannel("Colombo.Tests");

            Assert.That(() => ((IClientChannel)wcfService).RemoteAddress.Uri,
                Is.EqualTo(new Uri(@"http://localhost/Colombo.svc")));

            Assert.That(() => factory.CreateChannel("SomethingElse"),
                Throws.Exception.TypeOf<ColomboException>()
                .With.Message.Contains("SomethingElse"));
        }

        [Test]
        public void It_should_return_a_Channel_for_all_endpoints_from_configuration()
        {
            var factory = new WcfColomboServiceFactory();
            var allChannels = factory.CreateChannelsForAllEndPoints().ToArray();

            Assert.That(() => allChannels.Length,
                Is.EqualTo(3));

            Assert.That(() => ((IClientChannel)allChannels[0]).RemoteAddress.Uri,
                Is.EqualTo(new Uri(@"http://localhost/Colombo.svc")));

            Assert.That(() => ((IClientChannel)allChannels[1]).RemoteAddress.Uri,
                Is.EqualTo(new Uri(@"http://somewhereelse/Colombo.svc")));
        }
    }
}
