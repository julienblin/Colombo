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

using System.Linq;
using Colombo.Wcf;
using NUnit.Framework;

namespace Colombo.Tests.Wcf
{
    [TestFixture]
    public class ClientRestServiceTest
    {
        [Test]
        public void It_should_register_requests_and_clear_registrations()
        {
            ClientRestService.ClearRegistrations();

            ClientRestService.RegisterRequest<TestRequest>();
            ClientRestService.RegisterRequest<TestSideEffectFreeRequest>();

            Assert.That(ClientRestService.PostTypeMapping["Test"], Is.EqualTo(typeof(TestRequest)));
            Assert.That(ClientRestService.PostTypeMapping["TestRequest"], Is.EqualTo(typeof(TestRequest)));

            Assert.That(ClientRestService.GetTypeMapping["TestSideEffectFree"], Is.EqualTo(typeof(TestSideEffectFreeRequest)));
            Assert.That(ClientRestService.GetTypeMapping["TestSideEffectFreeRequest"], Is.EqualTo(typeof(TestSideEffectFreeRequest)));

            Assert.IsFalse(ClientRestService.PostTypeMapping.ContainsKey("TestSideEffectFree"));
            Assert.IsFalse(ClientRestService.PostTypeMapping.ContainsKey("TestSideEffectFreeRequest"));
            Assert.IsFalse(ClientRestService.GetTypeMapping.ContainsKey("Test"));
            Assert.IsFalse(ClientRestService.GetTypeMapping.ContainsKey("TestRequest"));

            var knownTypes = ClientRestService.GetKnownTypes(null).ToArray();

            Assert.That(knownTypes.Length, Is.EqualTo(3));
            Assert.That(knownTypes, Contains.Item(typeof(TestResponse)));
            Assert.That(knownTypes, Contains.Item(typeof(TestRequest)));
            Assert.That(knownTypes, Contains.Item(typeof(TestSideEffectFreeRequest)));

            ClientRestService.ClearRegistrations();
            knownTypes = ClientRestService.GetKnownTypes(null).ToArray();
            Assert.That(knownTypes.Length, Is.EqualTo(0));
            Assert.That(ClientRestService.GetTypeMapping.Count, Is.EqualTo(0));
            Assert.That(ClientRestService.PostTypeMapping.Count, Is.EqualTo(0));
        }

        [Test]
        public void It_should_raise_an_exception_if_request_already_registered()
        {
            ClientRestService.ClearRegistrations();

            ClientRestService.RegisterRequest<TestRequest>();
            Assert.That(() => ClientRestService.RegisterRequest<TestRequest>(),
                Throws.Exception.TypeOf<ColomboException>()
                .With.Message.Contains("TestRequest")
                .And.Message.Contains("Test"));

            ClientRestService.RegisterRequest<TestSideEffectFreeRequest>();
            Assert.That(() => ClientRestService.RegisterRequest<TestSideEffectFreeRequest>(),
                Throws.Exception.TypeOf<ColomboException>()
                .With.Message.Contains("TestSideEffectFreeRequest")
                .And.Message.Contains("TestSideEffectFree"));
        }
    }

    public class TestRequest : Request<TestResponse> {}

    public class TestSideEffectFreeRequest : SideEffectFreeRequest<TestResponse> { }
}
