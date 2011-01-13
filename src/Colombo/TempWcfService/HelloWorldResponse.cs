using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Colombo;

namespace TempWcfService
{
    public class HelloWorldResponse : Response
    {
        public virtual string Name { get; set; }
    }
}