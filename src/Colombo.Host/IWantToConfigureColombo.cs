﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.Windsor;

namespace Colombo.Host
{
    public interface IWantToConfigureColombo
    {
        void ConfigureColombo(IWindsorContainer container);
    }
}
