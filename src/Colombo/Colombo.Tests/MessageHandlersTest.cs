using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            var request = new TestRequest();
            request.CorrelationGuid = CorrelationGuid;
            request.Context["SomeKey"] = "SomeValue";

            requestHandler.Handle(request);
            Assert.That(() => requestHandler.HandleWasCalled,
                Is.True);
        }

        [Test]
        public void SideEffectFreeRequestHandler_inherited_functionality_should_work()
        {
            var sefRequestHandler = new TestSideEffectFreeRequestHandler();
            var sefRequest = new TestSideEffectFreeRequest();
            sefRequest.CorrelationGuid = CorrelationGuid;
            sefRequest.Context["SomeKey"] = "SomeValue";

            sefRequestHandler.Handle(sefRequest);
            Assert.That(() => sefRequestHandler.HandleWasCalled,
                Is.True);
        }

        [Test]
        public void NotificationHandler_inherited_functionality_should_work()
        {
            var notificationHandler = new TestNotificationHandler();
            var notification = new TestNotification();
            notification.CorrelationGuid = CorrelationGuid;
            notification.Context["SomeKey"] = "SomeValue";

            notificationHandler.Handle(notification);
            Assert.That(() => notificationHandler.HandleWasCalled,
                Is.True);
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
                Assert.That(() => Response.CorrelationGuid,
                    Is.EqualTo(CorrelationGuid));

                var newRequest = CreateRequest<TestRequest>();
                Assert.That(() => newRequest.CorrelationGuid,
                    Is.EqualTo(Request.CorrelationGuid));
                Assert.That(() => newRequest.Context,
                    Is.EqualTo(Request.Context));

                var newNotification = CreateNotification<TestNotification>();
                Assert.That(() => newNotification.CorrelationGuid,
                    Is.EqualTo(Request.CorrelationGuid));
                Assert.That(() => newNotification.Context,
                    Is.EqualTo(Request.Context));
            }
        }

        public class TestSideEffectFreeRequest : SideEffectFreeRequest<TestResponse> { }

        public class TestSideEffectFreeRequestHandler : SideEffectFreeRequestHandler<TestSideEffectFreeRequest, TestResponse>
        {
            public bool HandleWasCalled { get; set; }

            protected override void Handle()
            {
                HandleWasCalled = true;
                Assert.That(() => Response.CorrelationGuid,
                    Is.EqualTo(CorrelationGuid));

                var newRequest = CreateRequest<TestRequest>();
                Assert.That(() => newRequest.CorrelationGuid,
                    Is.EqualTo(Request.CorrelationGuid));
                Assert.That(() => newRequest.Context,
                    Is.EqualTo(Request.Context));

                var newNotification = CreateNotification<TestNotification>();
                Assert.That(() => newNotification.CorrelationGuid,
                    Is.EqualTo(Request.CorrelationGuid));
                Assert.That(() => newNotification.Context,
                    Is.EqualTo(Request.Context));

            }
        }

        public class TestNotification : Notification { }

        public class TestNotificationHandler : NotificationHandler<TestNotification>
        {
            public bool HandleWasCalled { get; set; }

            protected override void Handle()
            {
                HandleWasCalled = true;
                Assert.That(() => Notification.CorrelationGuid,
                    Is.EqualTo(CorrelationGuid));

                var newRequest = CreateRequest<TestRequest>();
                Assert.That(() => newRequest.CorrelationGuid,
                    Is.EqualTo(Notification.CorrelationGuid));
                Assert.That(() => newRequest.Context,
                    Is.EqualTo(Notification.Context));

                var newNotification = CreateNotification<TestNotification>();
                Assert.That(() => newNotification.CorrelationGuid,
                    Is.EqualTo(Notification.CorrelationGuid));
                Assert.That(() => newNotification.Context,
                    Is.EqualTo(Notification.Context));
            }
        }

        public delegate void HandleDelegate();
    }
}
