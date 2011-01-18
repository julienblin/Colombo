using System;

namespace Colombo.TestSupport
{
    /// <summary>
    /// Expectation for the Notify operation.
    /// </summary>
    /// <typeparam name="TNotification">The type of Notification that should be notified.</typeparam>
    public class MessageBusNotifyExpectation<TNotification> : BaseExpectation
        where TNotification : Notification, new()
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public MessageBusNotifyExpectation(IStubMessageBus stubMessageBus)
            : base(stubMessageBus)
        {
            ExpectedNumCalled = 1;
        }

        private TNotification receivedNotification;

        internal override object Execute(object parameter)
        {
            ++NumCalled;
            receivedNotification = (TNotification) parameter;
            return null;
        }

        /// <summary>
        /// Indicates that this expectation should be repeated n <paramref name="times"/>.
        /// </summary>
        public MessageBusNotifyExpectation<TNotification> Repeat(int times)
        {
            ExpectedNumCalled = times;
            return this;
        }

        private Action<TNotification> assertion;

        /// <summary>
        /// Allows to specify an action to assert certain properties on the notification.
        /// These asserts will be executed when <see cref="Verify"/> is called.
        /// </summary>
        public MessageBusNotifyExpectation<TNotification> Assert(Action<TNotification> action)
        {
            assertion = action;
            return this;
        }

        /// <summary>
        /// Verify that all the operations meet what this expectation planned.
        /// </summary>
        public override void Verify()
        {
            if (ExpectedNumCalled != NumCalled)
                throw new ColomboExpectationException(string.Format("Expected {0} to notify {1} time(s), actual: {2}", typeof(TNotification), ExpectedNumCalled, NumCalled));

            if((receivedNotification != null) && (assertion != null))
                assertion.Invoke(receivedNotification);
        }
    }
}
