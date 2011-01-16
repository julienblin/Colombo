using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Colombo.Impl;
using Rhino.Mocks;
using System.Threading;

namespace Colombo.Tests.Impl
{
    [TestFixture]
    public class LocalNotificationProcessorTest : BaseTest
    {
        [Test]
        public void It_should_ensure_that_it_has_a_INotificationHandlerFactory()
        {
            Assert.That(() => new LocalNotificationProcessor(null),
                Throws.Exception.TypeOf<ArgumentNullException>()
                .With.Message.Contains("notificationHandlerFactory"));
        }

        [Test]
        public void It_should_use_all_the_INotificationHandlers_that_INotificationHandlerFactory_returns()
        {
            var mocks = new MockRepository();

            var notificationHandlerFactory = mocks.StrictMock<INotificationHandlerFactory>();

            var notification1 = new TestNotification1();
            var notification2 = new TestNotification2();
            var notification3 = new TestNotification3();

            var notificationHandler1 = new TestNotificationHandler1();
            var notificationHandler2 = new TestNotificationHandler2();
            var notificationHandler22 = new TestNotificationHandler2();

            With.Mocks(mocks).Expecting(() =>
            {
                Expect.Call(notificationHandlerFactory.CanCreateNotificationHandlerFor(notification1)).Return(true).Repeat.Twice();
                Expect.Call(notificationHandlerFactory.CanCreateNotificationHandlerFor(notification2)).Return(true);
                Expect.Call(notificationHandlerFactory.CanCreateNotificationHandlerFor(notification3)).Return(false);

                Expect.Call(notificationHandlerFactory.CreateNotificationHandlersFor(notification1))
                    .Return(new INotificationHandler[] { notificationHandler1 })
                    .Repeat.Twice();

                notificationHandlerFactory.DisposeNotificationHandler(notificationHandler1);
                LastCall.Repeat.Twice();

                Expect.Call(notificationHandlerFactory.CreateNotificationHandlersFor(notification2))
                    .Return(new INotificationHandler[] { notificationHandler2, notificationHandler22 });

                notificationHandlerFactory.DisposeNotificationHandler(notificationHandler2);
                notificationHandlerFactory.DisposeNotificationHandler(notificationHandler22);

            }).Verify(() =>
            {
                var processor = new LocalNotificationProcessor(notificationHandlerFactory);
                processor.Logger = GetConsoleLogger();

                processor.Process(new Notification[] { notification1 });
                processor.Process(new Notification[] { notification1, notification2, notification3 });
                Thread.Sleep(500);

                Assert.That(() => notificationHandler1.NumHandleCalled,
                    Is.EqualTo(2));
                Assert.That(() => notificationHandler2.NumHandleCalled,
                    Is.EqualTo(1));
                Assert.That(() => notificationHandler22.NumHandleCalled,
                    Is.EqualTo(1));
            });
        }

        public class TestNotificationHandler1 : NotificationHandler<TestNotification1>
        {
            public int NumHandleCalled { get; private set; }

            protected override void Handle()
            {
                lock(this)
                    ++NumHandleCalled;
            }
        }

        public class TestNotificationHandler2 : NotificationHandler<TestNotification2>
        {
            public int NumHandleCalled { get; private set; }

            protected override void Handle()
            {
                lock (this)
                    ++NumHandleCalled;
            }
        }

        [Test]
        public void It_should_run_all_the_INotificationHandleInterceptors_registered()
        {
            var mocks = new MockRepository();

            var notificationHandlerFactory = mocks.StrictMock<INotificationHandlerFactory>();
            var notification = new TestNotification1();
            var notificationHandler1 = mocks.StrictMock<INotificationHandler<TestNotification1>>();
            var notificationHandler2 = mocks.StrictMock<INotificationHandler<TestNotification1>>();
            var interceptor1 = mocks.StrictMock<INotificationHandleInterceptor>();
            var interceptor2 = mocks.StrictMock<INotificationHandleInterceptor>();


            With.Mocks(mocks).Expecting(() =>
            {
                Expect.Call(interceptor1.InterceptionPriority).Return(InterceptionPrority.High);
                Expect.Call(interceptor2.InterceptionPriority).Return(InterceptionPrority.Medium);

                interceptor1.Intercept(null);
                LastCall.IgnoreArguments().Repeat.Twice().Do(new InterceptDelegate((invocation) =>
                {
                    invocation.Proceed();
                }));

                interceptor2.Intercept(null);
                LastCall.IgnoreArguments().Repeat.Twice().Do(new InterceptDelegate((invocation) =>
                {
                    invocation.Proceed();
                }));

                Expect.Call(notificationHandlerFactory.CanCreateNotificationHandlerFor(notification)).Return(true);
                Expect.Call(notificationHandlerFactory.CreateNotificationHandlersFor(notification))
                    .Return(new INotificationHandler[] { notificationHandler1, notificationHandler2 });

                notificationHandler1.Handle((Notification)notification);
                notificationHandler2.Handle((Notification)notification);

                notificationHandlerFactory.DisposeNotificationHandler(notificationHandler1);
                notificationHandlerFactory.DisposeNotificationHandler(notificationHandler2);
            }).Verify(() =>
            {
                var processor = new LocalNotificationProcessor(notificationHandlerFactory);
                processor.Logger = GetConsoleLogger();
                processor.NotificationHandleInterceptors = new INotificationHandleInterceptor[] { interceptor1, interceptor2 };

                processor.Process(new Notification[] { notification });
                Thread.Sleep(500);
            });
        }

        public class TestNotification1 : Notification { }

        public class TestNotification2 : Notification { }

        public class TestNotification3 : Notification { }

        public delegate void InterceptDelegate(IColomboNotificationHandleInvocation invocation);
    }
}
