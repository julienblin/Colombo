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
