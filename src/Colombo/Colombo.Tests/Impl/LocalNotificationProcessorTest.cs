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

            var notificationHandler1 = mocks.StrictMock<INotificationHandler<TestNotification1>>();
            var notificationHandler2 = mocks.StrictMock<INotificationHandler<TestNotification2>>();
            var notificationHandler22 = mocks.StrictMock<INotificationHandler<TestNotification2>>();

            With.Mocks(mocks).Expecting(() =>
            {
                Expect.Call(notificationHandlerFactory.CanCreateNotificationHandlerFor(notification1)).Return(true).Repeat.Twice();
                Expect.Call(notificationHandlerFactory.CanCreateNotificationHandlerFor(notification2)).Return(true);
                Expect.Call(notificationHandlerFactory.CanCreateNotificationHandlerFor(notification3)).Return(false);

                Expect.Call(notificationHandlerFactory.CreateNotificationHandlersFor(notification1))
                    .Return(new INotificationHandler[] { notificationHandler1 })
                    .Repeat.Twice();
                
                notificationHandler1.Handle((Notification)notification1);
                LastCall.Repeat.Twice();

                notificationHandlerFactory.DisposeNotificationHandler(notificationHandler1);
                LastCall.Repeat.Twice();

                Expect.Call(notificationHandlerFactory.CreateNotificationHandlersFor(notification2))
                    .Return(new INotificationHandler[] { notificationHandler2, notificationHandler22 });

                notificationHandler2.Handle((Notification)notification2);
                notificationHandler22.Handle((Notification)notification2);

                notificationHandlerFactory.DisposeNotificationHandler(notificationHandler2);
                notificationHandlerFactory.DisposeNotificationHandler(notificationHandler22);

            }).Verify(() =>
            {
                var processor = new LocalNotificationProcessor(notificationHandlerFactory);
                processor.Logger = GetConsoleLogger();

                processor.Process(new Notification[] { notification1 });
                processor.Process(new Notification[] { notification1, notification2, notification3 });

                Thread.Sleep(500);
            });
        }

        public class TestNotification1 : Notification { }

        public class TestNotification2 : Notification { }

        public class TestNotification3 : Notification { }
    }
}
