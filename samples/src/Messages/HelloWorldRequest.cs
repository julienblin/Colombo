using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace Colombo.Samples.Messages
{
    [SLA(200)]
    public class HelloWorldRequest : SideEffectFreeRequest<HelloWorldResponse>
    {
        [Required(AllowEmptyStrings = false)]
        [StringLength(maximumLength: 10)]
        public string Name { get; set; }
    }
}
