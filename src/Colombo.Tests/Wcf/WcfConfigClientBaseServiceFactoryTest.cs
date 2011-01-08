using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Colombo.Wcf;

namespace Colombo.Tests.Wcf
{
    [TestFixture]
    public class WcfConfigClientBaseServiceFactoryTest : BaseTest
    {
        [Test]
        public void It_should_check_WCF_client_configuration_to_determine_if_it_can_create_ClientBase()
        {
            var factory = new WcfServiceFactory();
            Assert.That(() => factory.CanCreateClientBaseForRequestGroup("Colombo.Tests"),
                Is.True);
            Assert.That(() => factory.CanCreateClientBaseForRequestGroup("AnotherGroupName"),
                Is.False);
        }

        [Test]
        public void It_should_return_address_from_WCF_configuration()
        {
            var factory = new WcfServiceFactory();
            Assert.That(() => factory.GetAddressForRequestGroup("Colombo.Tests"),
                Is.EqualTo(@"http://localhost/Colombo.svc"));
            Assert.That(() => factory.GetAddressForRequestGroup("AnotherAssembly"),
                Is.EqualTo(@"http://somewhereelse/Colombo.svc"));
            Assert.That(() => factory.GetAddressForRequestGroup("SomethingElse"),
                Is.Null);
        }

        [Test]
        public void It_should_create_a_ClientBase_from_configuration()
        {
            var factory = new WcfServiceFactory();

            var clientBase = factory.CreateClientBase("Colombo.Tests");

            //Assert.That(() => clientBase.Endpoint.Address.Uri,
            //    Is.EqualTo(new Uri(@"http://localhost/Colombo.svc")));

            Assert.That(() => factory.CreateClientBase("SomethingElse"),
                Throws.Exception.TypeOf<ColomboException>()
                .With.Message.Contains("SomethingElse"));
        }
    }
}
