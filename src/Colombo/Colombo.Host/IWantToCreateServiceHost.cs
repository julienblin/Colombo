﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Windsor;

namespace Colombo.Host
{
    public interface IWantToCreateServiceHost
    {
        System.ServiceModel.ServiceHost CreateServiceHost(IWindsorContainer container);
    }
}