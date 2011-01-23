using System;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Colombo.Impl.NotificationHandle;
using NUnit.Framework;
using Rhino.Mocks;

namespace Colombo.Tests.Impl
{
    [TestFixture]
    public class KernelNotificationHandlerFactoryTest : BaseTest
    {
        [Test]
        public void It_should_ensure_that_it_has_a_IKernel()
        {
            Assert.That(() => new KernelNotificationHandlerFactory(null),
                Throws.Exception.TypeOf<ArgumentNullException>()
                .With.Message.Contains("kernel"));
        }

        [Test]
        public void It_should_respond_to_cancreaterequesthandlerfor_depending_on_the_kernel()
        {
            var mocks = new MockRepository();
            var notificationHandler = mocks.Stub<INotificationHandler<TestNotification>>();

            var kernel = new DefaultKernel();
            kernel.Register(
                Component.For<INotificationHandler<TestNotification>>().Instance(notificationHandler)
            );

            var factory = new KernelNotificationHandlerFactory(kernel);
            Assert.That(() => factory.CanCreateNotificationHandlerFor(new TestNotification()),
                Is.True);
            Assert.That(() => factory.CanCreateNotificationHandlerFor(new TestNotification2()),
                Is.False);
        }

        [Test]
        public void It_should_use_the_kernel_to_create_INotificationHandlers()
        {
            var kernel = new DefaultKernel();
            kernel.Register(
                Component.For<INotificationHandler<TestNotification>>()
                    .LifeStyle.Transient
                    .ImplementedBy<TestNotificationHandler>(),
                Component.For<INotificationHandler<TestNotification>>()
                    .LifeStyle.Transient
                    .ImplementedBy<TestNotificationHandler2>()
            );

            var factory = new KernelNotificationHandlerFactory(kernel);
            var notificationhandlers = factory.CreateNotificationHandlersFor(new TestNotification());
            Assert.That(() => notificationhandlers.Length,
                Is.EqualTo(2));
            Assert.That(() => notificationhandlers[0],
                Is.TypeOf<TestNotificationHandler>());
            Assert.That(() => notificationhandlers[1],
                Is.TypeOf<TestNotificationHandler2>());

            notificationhandlers = factory.CreateNotificationHandlersFor(new TestNotification2());
            Assert.That(() => notificationhandlers.Length,
                Is.EqualTo(0));
        }

        public class TestNotification : Notification { }

        public class TestNotification2 : Notification { }

        public class TestNotificationHandler : NotificationHandler<TestNotification>
        {
            protected override void Handle()
            {
                throw new NotImplementedException();
            }
        }

        public class TestNotificationHandler2 : NotificationHandler<TestNotification>
        {
            protected override void Handle()
            {
                throw new NotImplementedException();
            }
        }
    }
}
