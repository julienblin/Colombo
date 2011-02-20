#region License
// The MIT License
// 
// Copyright (c) 2011 Julien Blin, julien.blin@gmail.com
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
#endregion

using System;
using System.Linq;
using System.ServiceModel;
using Colombo.Wcf;
using NUnit.Framework;

namespace Colombo.Tests.Wcf
{
    [TestFixture]
    public class ColomboServiceFactoryTest : BaseTest
    {
        [Test]
        public void It_should_check_WCF_client_configuration_to_determine_if_it_can_create_Channel()
        {
            var factory = new ColomboServiceFactory();
            Assert.That(factory.CanCreateChannelForRequestGroup("Colombo.Tests"), Is.True);
            Assert.That(factory.CanCreateChannelForRequestGroup("AnotherGroupName"), Is.False);
        }

        [Test]
        public void It_should_return_address_from_WCF_configuration()
        {
            var factory = new ColomboServiceFactory();
            Assert.That(factory.GetAddressForRequestGroup("Colombo.Tests"), Is.EqualTo(@"http://localhost/Colombo.svc"));
            Assert.That(factory.GetAddressForRequestGroup("AnotherAssembly"), Is.EqualTo(@"http://somewhereelse/Colombo.svc"));
            Assert.That(factory.GetAddressForRequestGroup("SomethingElse"), Is.Null);
        }

        [Test]
        public void It_should_create_a_Channel_from_configuration()
        {
            var factory = new ColomboServiceFactory();

            var wcfService = factory.CreateChannel("Colombo.Tests");

            Assert.That(((IClientChannel)wcfService).RemoteAddress.Uri, Is.EqualTo(new Uri(@"http://localhost/Colombo.svc")));

            Assert.That(() => factory.CreateChannel("SomethingElse"),
                Throws.Exception.TypeOf<ColomboException>()
                .With.Message.Contains("SomethingElse"));
        }

        [Test]
        public void It_should_return_a_Channel_for_all_endpoints_from_configuration()
        {
            var factory = new ColomboServiceFactory();
            var allChannels = factory.CreateChannelsForAllEndPoints().ToArray();

            Assert.That(allChannels.Length, Is.EqualTo(3));
            Assert.That(((IClientChannel)allChannels[0]).RemoteAddress.Uri, Is.EqualTo(new Uri(@"http://localhost/Colombo.svc")));
            Assert.That(((IClientChannel)allChannels[1]).RemoteAddress.Uri, Is.EqualTo(new Uri(@"http://somewhereelse/Colombo.svc")));
        }
    }
}
