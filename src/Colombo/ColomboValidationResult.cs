using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Colombo
{
    /// <summary>
    /// Serailization-friendly version of <see cref="ValidationResult"/>.
    /// </summary>
    [DataContract]
    public class ColomboValidationResult
    {
        public ColomboValidationResult(string errorMessage)
            :this(errorMessage, null)
        {
        }

        public ColomboValidationResult(ValidationResult validationResult)
            : this(validationResult.ErrorMessage, validationResult.MemberNames)
        {
        }

        public ColomboValidationResult(string errorMessage, IEnumerable<string> memberNames)
        {
            ErrorMessage = errorMessage;
            if(memberNames != null)
                MemberNames = new List<string>(memberNames);
            else
                MemberNames = new List<string>();
        }

        [DataMember]
        public string ErrorMessage { get; set; }

        [DataMember]
        public IEnumerable<string> MemberNames { get; set; }

        public ValidationResult ToValidationResult()
        {
            return new ValidationResult(ErrorMessage, MemberNames);
        }
    }
}
