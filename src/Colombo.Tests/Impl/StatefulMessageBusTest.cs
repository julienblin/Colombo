﻿#region License
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
using System.Collections.Generic;
using Colombo.Impl;
using NUnit.Framework;
using Rhino.Mocks;

namespace Colombo.Tests.Impl
{
    [TestFixture]
    public class StatefulMessageBusTest
    {
        [Test]
        public void It_should_ensure_that_a_IMessageBus_is_provided()
        {
            Assert.That(() => new StatefulMessageBus(null),
                Throws.Exception.TypeOf<ArgumentNullException>()
                .With.Message.Contains("messageBus"));
        }

        [Test]
        public void It_should_throw_an_exception_if_any_response_member_is_not_virtual()
        {
            var mocks = new MockRepository();
            var messageBus = mocks.StrictMock<IMessageBus>();

            var statefulMB = new StatefulMessageBus(messageBus);

            Assert.That(() => statefulMB.FutureSend(new TestRequestNonVirtual()),
                Throws.Exception.TypeOf<ColomboException>()
                .With.Message.Contains("virtual")
                .And.Message.Contains("get_NameIncorrect"));
        }

        [Test]
        public void It_should_delegate_to_message_bus_for_IMessageBus()
        {
            var mocks = new MockRepository();

            var messageBus = mocks.StrictMock<IMessageBus>();

            With.Mocks(mocks).Expecting(() =>
            {
                Expect.Call(messageBus.Send(new TestRequest())).IgnoreArguments().Return(new TestResponse());
                Expect.Call(messageBus.Send(new TestSideEffectFreeRequest())).IgnoreArguments().Return(new TestResponse());
                Expect.Call(messageBus.Send(new TestSideEffectFreeRequest(), new TestSideEffectFreeRequest())).IgnoreArguments().Return(new ResponsesGroup());
            }).Verify(() =>
            {
                var statefulMB = new StatefulMessageBus(messageBus);
                statefulMB.Send(new TestRequest());
                statefulMB.Send(new TestSideEffectFreeRequest());
                statefulMB.Send(new TestSideEffectFreeRequest(), new TestSideEffectFreeRequest());
            });
        }

        [Test]
        public void It_should_not_send_immediately_and_return_a_proxy()
        {
            var messageBus = new TestMessageBus();
            var statefulMB = new StatefulMessageBus(messageBus);
            var response = statefulMB.FutureSend(new TestSideEffectFreeRequest());

            Assert.That(response.GetType(), Is.Not.EqualTo(typeof(TestResponse)));
            Assert.That(response, Is.AssignableTo<TestResponse>());
            Assert.That(response, Is.Not.Null);

            Assert.That(statefulMB.NumberOfSend, Is.EqualTo(0));
            Assert.That(messageBus.NumSendCalled, Is.EqualTo(0));
        }

        [Test]
        public void It_should_send_when_accessing_a_proxy()
        {
            var request = new TestSideEffectFreeRequest();
            var response = new TestResponse();
            response.CorrelationGuid = request.CorrelationGuid;
            var messageBus = new TestMessageBus(response);
            var statefulMB = new StatefulMessageBus(messageBus);
            var responseProxy = statefulMB.FutureSend(request);

            Assert.That(statefulMB.NumberOfSend, Is.EqualTo(0));
            Assert.That(responseProxy.CorrelationGuid, Is.EqualTo(request.CorrelationGuid));
            Assert.That(messageBus.NumSendCalled, Is.EqualTo(1));
            Assert.That(statefulMB.NumberOfSend, Is.EqualTo(1));
        }

        [Test]
        public void It_should_batch_future_send()
        {
            var request1 = new TestSideEffectFreeRequest();
            var request2 = new TestSideEffectFreeRequest();
            var response1 = new TestResponse { CorrelationGuid = request1.CorrelationGuid };
            var response2 = new TestResponse { CorrelationGuid = request2.CorrelationGuid };

            var messageBus = new TestMessageBus(response1, response2);
            var statefulMB = new StatefulMessageBus(messageBus);
            var responseProxy1 = statefulMB.FutureSend(request1);
            var responseProxy2 = statefulMB.FutureSend(request2);

            Assert.That(statefulMB.NumberOfSend, Is.EqualTo(0));
            Assert.That(responseProxy1.CorrelationGuid, Is.EqualTo(request1.CorrelationGuid));
            Assert.That(responseProxy2.CorrelationGuid, Is.EqualTo(request2.CorrelationGuid));
            Assert.That(messageBus.NumSendCalled, Is.EqualTo(1));
            Assert.That(statefulMB.NumberOfSend, Is.EqualTo(1));
        }

        [Test]
        public void It_should_throw_an_exception_when_send_quota_is_over()
        {
            var request1 = new TestSideEffectFreeRequest();
            var request2 = new TestSideEffectFreeRequest();
            var response1 = new TestResponse { CorrelationGuid = request1.CorrelationGuid };

            var messageBus = new TestMessageBus(response1);
            var statefulMB = new StatefulMessageBus(messageBus);
            statefulMB.MaxAllowedNumberOfSend = 1;
            var responseProxy1 = statefulMB.FutureSend(request1);

            Assert.That(responseProxy1.CorrelationGuid, Is.EqualTo(request1.CorrelationGuid));

            Assert.That(() => statefulMB.FutureSend(request2),
                Throws.Exception.TypeOf<ColomboException>()
                .With.Message.Contains("MaxAllowedNumberOfSend"));
        }

        [Test]
        public void It_should_not_throw_an_exception_when_send_quota_is_not_over()
        {
            var request1 = new TestSideEffectFreeRequest();
            var response1 = new TestResponse { CorrelationGuid = request1.CorrelationGuid };

            var messageBus = new TestMessageBus(response1);
            var statefulMB = new StatefulMessageBus(messageBus) { MaxAllowedNumberOfSend = 1 };
            var responseProxy1 = statefulMB.FutureSend(request1);

            Assert.That(responseProxy1.CorrelationGuid, Is.EqualTo(request1.CorrelationGuid));
            Assert.That(statefulMB.NumberOfSend, Is.EqualTo(1));
        }

        [Test]
        public void It_should_not_throw_an_exception_when_send_quota_is_zero_or_negative()
        {
            var request1 = new TestSideEffectFreeRequest();
            var response1 = new TestResponse { CorrelationGuid = request1.CorrelationGuid };

            var messageBus = new TestMessageBus(response1);
            var statefulMB = new StatefulMessageBus(messageBus);
            statefulMB.MaxAllowedNumberOfSend = 0;
            var responseProxy1 = statefulMB.FutureSend(request1);

            Assert.That(responseProxy1.CorrelationGuid, Is.EqualTo(request1.CorrelationGuid));
        }

        [Test]
        public void It_should_preserve_original_stack_trace_when_response_instance_throws_Exception()
        {
            string[] stackTraceOriginalLines = null;

            try
            {
                var responseStackTrace = new TestResponseException();
                var exceptionAccessStackTrace = responseStackTrace.UtcTimestamp;
            }
            catch (Exception ex)
            {
                stackTraceOriginalLines = ex.StackTrace.Split('\n');
            }

            var request = new TestRequestException();
            var response = new TestResponseException();
            var messageBus = new TestMessageBus(response);
            var statefulMB = new StatefulMessageBus(messageBus);
            var responseProxy = statefulMB.FutureSend(request);

            var guidAccess = responseProxy.CorrelationGuid;

            try
            {
                var exceptionAccess = responseProxy.UtcTimestamp;
            }
            catch (Exception ex)
            {
                var proxyStackStraceLines = ex.StackTrace.Split('\n');
                Assert.That(proxyStackStraceLines[0], Is.EqualTo(stackTraceOriginalLines[0]));
            }
        }

        [Test]
        public void It_should_allow_initialization_of_virtual_members_in_ctor()
        {
            var messageBus = new TestMessageBus();
            var statefulMB = new StatefulMessageBus(messageBus);

            Assert.DoesNotThrow(() => statefulMB.FutureSend(new TestRequestWithInitInCtor()));
        }

        public class TestResponseNonVirtual : Response
        {
            public virtual string Name { get; set; }

            public string NameIncorrect { get; set; }
        }

        public class TestRequestNonVirtual : SideEffectFreeRequest<TestResponseNonVirtual>
        {
        }

        public class TestRequest : Request<TestResponse>
        {
        }

        public class TestSideEffectFreeRequest : SideEffectFreeRequest<TestResponse>
        {
        }

        public class TestResponseException : Response
        {
            public override DateTime UtcTimestamp
            {
                get
                {
                    throw new ApplicationException("This is the internal error");
                }
                set
                {
                    base.UtcTimestamp = value;
                }
            }
        }

        public class TestRequestException : SideEffectFreeRequest<TestResponseException>
        {
        }

        public class TestResponseWithInitInCtor : Response
        {
            public TestResponseWithInitInCtor()
            {
                Values = new List<string>();
            }

            public virtual IEnumerable<string> Values { get; set; }
        }

        public class TestRequestWithInitInCtor : SideEffectFreeRequest<TestResponseWithInitInCtor>
        {
            
        }

        public class TestMessageBus : IMessageBus
        {
            Response[] responses;
            public int NumSendCalled { get; set; }

            public TestMessageBus(params Response[] responses)
            {
                this.responses = responses;
                NumSendCalled = 0;
            }

            public Response Send(BaseRequest request)
            {
                throw new NotImplementedException();
            }

            public TResponse Send<TResponse>(Request<TResponse> request)
                where TResponse : Response, new()
            {
                throw new NotImplementedException();
            }

            public IAsyncCallback<TResponse> SendAsync<TResponse>(Request<TResponse> request)
                where TResponse : Response, new()
            {
                throw new NotImplementedException();
            }

            public TResponse Send<TResponse>(SideEffectFreeRequest<TResponse> request)
                where TResponse : Response, new()
            {
                throw new NotImplementedException();
            }

            public TResponse Send<TRequest, TResponse>(Action<TRequest> action)
                where TRequest : SideEffectFreeRequest<TResponse>, new()
                where TResponse : Response, new()
            {
                throw new NotImplementedException();
            }

            public IAsyncCallback<TResponse> SendAsync<TResponse>(SideEffectFreeRequest<TResponse> request)
                where TResponse : Response, new()
            {
                throw new NotImplementedException();
            }

            public ResponsesGroup Send(BaseSideEffectFreeRequest request, params BaseSideEffectFreeRequest[] followingRequests)
            {
                ++NumSendCalled;

                var requestList = new List<BaseSideEffectFreeRequest>();
                requestList.Add(request);
                requestList.AddRange(followingRequests);
                var responsesGroup = new ResponsesGroup();
                for (var i = 0; i < responses.Length; i++)
                {
                    responsesGroup[requestList[i]] = responses[i];
                }
                return responsesGroup;
            }
        }
    }
}
