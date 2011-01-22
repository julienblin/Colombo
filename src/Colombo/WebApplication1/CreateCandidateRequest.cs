using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Colombo;

namespace WebApplication1
{
    public class CreateCandidateRequest : Request<CreateCandidateResponse>
    {
        [Required(AllowEmptyStrings = false)]
        public string Name { get; set; }
    }
}