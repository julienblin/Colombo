﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Colombo.Samples.Messages
{
    public class HelloWorldResponse : ValidatedResponse
    {
        public string Message { get; set; }
    }
}
