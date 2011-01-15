﻿namespace Colombo.Impl.NotificationHandle
{
    public abstract class BaseNotificationHandleInvocation : IColomboNotificationHandleInvocation
    {
        public Notification Notification { get; set; }

        public abstract void Proceed();
    }
}
