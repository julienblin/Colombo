﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Colombo
{
    public interface IColomboInvocation
    {
        BaseRequest Request { get; }
        Response Response { get; set; }
        void Proceed();
    }
}