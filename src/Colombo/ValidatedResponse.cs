using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;

namespace Colombo
{
    public abstract class ValidatedResponse : Response
    {
        private IList<ColomboValidationResult> validationResults;

        public virtual IList<ColomboValidationResult> ValidationResults
        {
            get
            {
                if (validationResults == null)
                    validationResults = new List<ColomboValidationResult>();
                return validationResults;
            }
            set
            {
                validationResults = value;
            }
        }

        public virtual bool IsValid()
        {
            return (ValidationResults.Count == 0);
        }
    }
}
