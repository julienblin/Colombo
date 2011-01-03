using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Colombo.Samples.Messages
{
    public class HelloWorldRequest : Request<HelloWorldResponse>
    {
        public string Name { get; set; }
    }
}
