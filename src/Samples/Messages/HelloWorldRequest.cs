using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using Colombo.Interceptors;

namespace Colombo.Samples.Messages
{
    [SLA(500)]
    //[RequiredInContext(CurrentCultureConstant.CultureContextKey)]
    public class HelloWorldRequest : Request<HelloWorldResponse>
    {
        [Required]
        [StringLength(10)]
        public string Name { get; set; }
    }
}
