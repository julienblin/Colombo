using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Colombo
{
    public abstract class Response : Message
    {
        public Response()
        {
            ValidationResults = new List<ValidationResult>();
        }

        public ICollection<ValidationResult> ValidationResults { get; set; }

        public virtual bool IsValid()
        {
            if (ValidationResults == null)
                return true;

            return (ValidationResults.Count == 0);
        }
    }
}
