﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;

namespace Colombo
{
    [ContractClass(typeof(Contracts.NotificationProcessorContract))]
    public interface INotificationProcessor
    {
        void Process(Notification[] notifications);
    }
}
