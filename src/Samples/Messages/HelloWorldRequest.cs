using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Colombo.Samples.Messages
{
    [SLA(500)]
    public class HelloWorldRequest : Request<HelloWorldResponse>
    {
        [Required]
        [StringLength(10)]
        public string Name { get; set; }
    }
}
