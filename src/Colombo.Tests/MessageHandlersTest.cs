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
    public class MessageHandlersTest
    {
        static Guid CorrelationGuid = Guid.NewGuid();

        [Test]
        public void RequestHandler_inherited_functionality_should_work()
        {
            var requestHandler = new TestRequestHandler();
            var request = new TestRequest { CorrelationGuid = CorrelationGuid };
            request.Context["SomeKey"] = "SomeValue";

            requestHandler.Handle(request);

            Assert.That(requestHandler.HandleWasCalled, Is.True);
        }

        [Test]
        public void SideEffectFreeRequestHandler_inherited_functionality_should_work()
        {
            var sefRequestHandler = new TestSideEffectFreeRequestHandler();
            var sefRequest = new TestSideEffectFreeRequest { CorrelationGuid = CorrelationGuid };
            sefRequest.Context["SomeKey"] = "SomeValue";

            sefRequestHandler.Handle(sefRequest);

            Assert.That(sefRequestHandler.HandleWasCalled,Is.True);
        }

        [Test]
        public void SideEffectFreeRequestHandler_inherited_should_SetPaginationInfo()
        {
            var sefRequestHandler = new TestPaginatedRequestHandler();
            var sefRequest = new TestPaginatedRequest { CurrentPage = 2, PerPage = 50 };

            sefRequestHandler.Handle(sefRequest);

            Assert.That(sefRequestHandler.HandleWasCalled, Is.True);
        }

        public class TestRequest : Request<TestResponse>
        {
        }

        public class TestRequestHandler : RequestHandler<TestRequest, TestResponse>
        {
            public bool HandleWasCalled { get; set; }

            protected override void Handle()
            {
                HandleWasCalled = true;
                Assert.That(Response.CorrelationGuid, Is.EqualTo(CorrelationGuid));

                var newRequest = CreateRequest<TestRequest>();
                Assert.That(newRequest.CorrelationGuid, Is.EqualTo(Request.CorrelationGuid));
                Assert.That(newRequest.Context, Is.EqualTo(Request.Context));
            }
        }

        public class TestSideEffectFreeRequest : SideEffectFreeRequest<TestResponse> { }

        public class TestSideEffectFreeRequestHandler : SideEffectFreeRequestHandler<TestSideEffectFreeRequest, TestResponse>
        {
            public bool HandleWasCalled { get; set; }

            protected override void Handle()
            {
                HandleWasCalled = true;
                Assert.That(Response.CorrelationGuid, Is.EqualTo(CorrelationGuid));

                var newRequest = CreateRequest<TestRequest>();
                Assert.That(newRequest.CorrelationGuid, Is.EqualTo(Request.CorrelationGuid));
                Assert.That(newRequest.Context, Is.EqualTo(Request.Context));
            }
        }

        public class TestPaginatedResponse : PaginatedResponse { }

        public class TestPaginatedRequest : PaginatedRequest<TestPaginatedResponse> { }

        public class TestPaginatedRequestHandler : SideEffectFreeRequestHandler<TestPaginatedRequest, TestPaginatedResponse>
        {
            public bool HandleWasCalled { get; set; }

            protected override void Handle()
            {
                HandleWasCalled = true;
                SetPaginationInfo(230);
                Assert.That(Response.CurrentPage, Is.EqualTo(Request.CurrentPage));
                Assert.That(Response.PerPage, Is.EqualTo(Request.PerPage));
                Assert.That(Response.TotalEntries, Is.EqualTo(230));

                var fullyFilledPages = Response.TotalEntries / Response.PerPage;
                var remainingPage = ((Response.TotalEntries % Response.PerPage) > 0) ? 1 : 0;
                var expectedTotalPages = fullyFilledPages + remainingPage;

                Assert.That(Response.TotalPages, Is.EqualTo(expectedTotalPages));
            }
        }



        public delegate void HandleDelegate();
    }
}
