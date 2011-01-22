using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Colombo.Wcf;
using NUnit.Framework;

namespace Colombo.Tests.Wcf
{
    [TestFixture]
    public class WcfJsBridgeServiceTest
    {
        [Test]
        public void It_should_register_requests_and_clear_registrations()
        {
            WcfJsBridgeService.ClearRegistrations();

            WcfJsBridgeService.RegisterRequest<TestRequest>();
            WcfJsBridgeService.RegisterRequest<TestSideEffectFreeRequest>();

            Assert.That(() => WcfJsBridgeService.PostTypeMapping["Test"],
                Is.EqualTo(typeof(TestRequest)));
            Assert.That(() => WcfJsBridgeService.PostTypeMapping["TestRequest"],
                Is.EqualTo(typeof(TestRequest)));

            Assert.That(() => WcfJsBridgeService.GetTypeMapping["TestSideEffectFree"],
                Is.EqualTo(typeof(TestSideEffectFreeRequest)));
            Assert.That(() => WcfJsBridgeService.GetTypeMapping["TestSideEffectFreeRequest"],
                Is.EqualTo(typeof(TestSideEffectFreeRequest)));

            Assert.IsFalse(WcfJsBridgeService.PostTypeMapping.ContainsKey("TestSideEffectFree"));
            Assert.IsFalse(WcfJsBridgeService.PostTypeMapping.ContainsKey("TestSideEffectFreeRequest"));
            Assert.IsFalse(WcfJsBridgeService.GetTypeMapping.ContainsKey("Test"));
            Assert.IsFalse(WcfJsBridgeService.GetTypeMapping.ContainsKey("TestRequest"));

            var knownTypes = WcfJsBridgeService.GetKnownTypes(null).ToArray();

            Assert.That(() => knownTypes.Length, Is.EqualTo(3));
            Assert.That(() => knownTypes, Contains.Item(typeof(TestResponse)));
            Assert.That(() => knownTypes, Contains.Item(typeof(TestRequest)));
            Assert.That(() => knownTypes, Contains.Item(typeof(TestSideEffectFreeRequest)));

            WcfJsBridgeService.ClearRegistrations();
            knownTypes = WcfJsBridgeService.GetKnownTypes(null).ToArray();
            Assert.That(() => knownTypes.Length, Is.EqualTo(0));
            Assert.That(() => WcfJsBridgeService.GetTypeMapping.Count, Is.EqualTo(0));
            Assert.That(() => WcfJsBridgeService.PostTypeMapping.Count, Is.EqualTo(0));
        }

        [Test]
        public void It_should_raise_an_exception_if_request_already_registered()
        {
            WcfJsBridgeService.ClearRegistrations();

            WcfJsBridgeService.RegisterRequest<TestRequest>();
            Assert.That(() => WcfJsBridgeService.RegisterRequest<TestRequest>(),
                Throws.Exception.TypeOf<ColomboException>()
                .With.Message.Contains("TestRequest")
                .And.Message.Contains("Test"));

            WcfJsBridgeService.RegisterRequest<TestSideEffectFreeRequest>();
            Assert.That(() => WcfJsBridgeService.RegisterRequest<TestSideEffectFreeRequest>(),
                Throws.Exception.TypeOf<ColomboException>()
                .With.Message.Contains("TestSideEffectFreeRequest")
                .And.Message.Contains("TestSideEffectFree"));
        }
    }

    public class TestRequest : Request<TestResponse> {}

    public class TestSideEffectFreeRequest : SideEffectFreeRequest<TestResponse> { }
}
