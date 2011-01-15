﻿using System;
using System.Diagnostics.Contracts;

namespace Colombo.Contracts
{
    [ContractClassFor(typeof(INotificationHandler<>))]
    public abstract class GenericNotificationHandler<TNotification> : INotificationHandler<TNotification>
        where TNotification : Notification
    {
        public void Handle(TNotification notification)
        {
            throw new NotImplementedException();
        }

        public void Handle(Notification notification)
        {
            throw new NotImplementedException();
        }
    }
}
