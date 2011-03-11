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

using System.ComponentModel.DataAnnotations;
using Colombo.TestSupport;
using NUnit.Framework;

namespace Colombo.Tests.TestSupport
{
    [TestFixture]
    public class ColomboTestTest
    {
        [Test]
        public void It_should_reject_requests_that_cannot_be_created_using_Activator()
        {
            Assert.That(() => ColomboTest.AssertThat.RequestIsConform<RequestWithoutDefaultConstructor>(),
                Throws.Exception.TypeOf<ColomboTestSupportException>()
                .With.Message.Contains("default constructor"));
        }

        [Test]
        public void It_should_reject_requests_that_are_not_serializable_with_DataContractSerializer()
        {
            Assert.That(() => ColomboTest.AssertThat.RequestIsConform<RequestNotSerializable>(),
                Throws.Exception.TypeOf<ColomboTestSupportException>()
                .With.Message.Contains("DataContractSerializer"));
        }

        [Test]
        public void It_should_reject_responses_that_cannot_be_created_using_Activator()
        {
            Assert.That(() => ColomboTest.AssertThat.ResponseIsConform<ResponseWithoutDefaultConstructor>(),
                Throws.Exception.TypeOf<ColomboTestSupportException>()
                .With.Message.Contains("default constructor"));
        }

        [Test]
        public void It_should_reject_responses_that_are_not_serializable_with_DataContractSerializer()
        {
            Assert.That(() => ColomboTest.AssertThat.ResponseIsConform<ResponseNotSerializable>(),
                Throws.Exception.TypeOf<ColomboTestSupportException>()
                .With.Message.Contains("DataContractSerializer"));
        }

        [Test]
        public void It_should_reject_responses_when_a_member_is_not_virtual()
        {
            Assert.That(() => ColomboTest.AssertThat.ResponseIsConform<ResponseWithNonVirtualMember>(),
                Throws.Exception.TypeOf<ColomboTestSupportException>()
                .With.Message.Contains("virtual"));
        }

        [Test]
        public void It_should_verify_that_requests_with_EnableCache_are_sideeffectfree()
        {
            Assert.That(() => ColomboTest.AssertThat.RequestIsConform<RequestEnableCacheNotSideEffectFree>(),
                Throws.Exception.TypeOf<ColomboTestSupportException>()
                .With.Message.Contains("EnableCache"));
        }

        [Test]
        public void It_should_verify_that_requests_with_EnableCache_implements_GetCacheKey()
        {
            Assert.That(() => ColomboTest.AssertThat.RequestIsConform<RequestEnableCacheNotGetCacheKeyImplementation>(),
               Throws.Exception.TypeOf<ColomboTestSupportException>()
               .With.Message.Contains("EnableCache")
               .And.Message.Contains("GetCacheKey"));
        }

        public class RequestWithoutDefaultConstructor : Request<TestResponse>
        {
            public RequestWithoutDefaultConstructor(string name)
            {
            }
        }

        public class RequestNotSerializable : Request<TestResponse>
        {
            public ValidationResult ValidationResult { get; set; }
        }

        public class ResponseWithoutDefaultConstructor : Response
        {
            public ResponseWithoutDefaultConstructor(string name)
            {
                    
            }
        }

        public class ResponseNotSerializable : Response
        {
            public virtual ValidationResult ValidationResult { get; set; }
        }

        public class ResponseWithNonVirtualMember : Response
        {
            public string Name { get; set; }
        }

        [EnableCache]
        public class RequestEnableCacheNotSideEffectFree : Request<TestResponse> { }

        [EnableCache]
        public class RequestEnableCacheNotGetCacheKeyImplementation : SideEffectFreeRequest<TestResponse> { }
    }
}
