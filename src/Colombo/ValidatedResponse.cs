using System.Collections.Generic;
using System.Runtime.Serialization;

namespace Colombo
{
    /// <summary>
    /// Base class for responses that contains validation information.
    /// Based on DataAnnotations format.
    /// </summary>
    [DataContract]
    public abstract class ValidatedResponse : Response
    {
        private IList<System.ComponentModel.DataAnnotations.ValidationResult> internalValidationResults;
        /// <summary>
        /// List of all the <see cref="System.ComponentModel.DataAnnotations.ValidationResult"/>.
        /// </summary>
        [IgnoreDataMember]
        public virtual IList<System.ComponentModel.DataAnnotations.ValidationResult> ValidationResults
        {
            get {
                return internalValidationResults ??
                       (internalValidationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>());
            }
        }

        /// <summary>
        /// <c>true</c> is the request was valid, <c>false</c> otherwise.
        /// </summary>
        public virtual bool IsValid()
        {
            return (ValidationResults.Count == 0);
        }

        #region Serializing ValidationResult

        /// <summary>
        /// Holds serialization-friendly <see cref="ValidationResult"/>.
        /// </summary>
        [DataMember(Name="ValidationResults")]
        private List<ValidationResult> serializableValidationResults;

        [OnSerializing]
        private void OnSerializing(StreamingContext context)
        {
            serializableValidationResults = new List<ValidationResult>();
            foreach (var validationResult in ValidationResults)
                serializableValidationResults.Add(new ValidationResult(validationResult));
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            foreach (var validationResultSerialization in serializableValidationResults)
            {
                ValidationResults.Add(new System.ComponentModel.DataAnnotations.ValidationResult(validationResultSerialization.ErrorMessage, validationResultSerialization.MemberNames));
            }
        }

        /// <summary>
        /// Serialization friendly version of <see cref="System.ComponentModel.DataAnnotations.ValidationResult"/>
        /// </summary>
        [DataContract(Name="ValidationResult")]
        private class ValidationResult
        {
            public ValidationResult()
            {
            }

            public ValidationResult(System.ComponentModel.DataAnnotations.ValidationResult validationResult)
            {
                ErrorMessage = validationResult.ErrorMessage;
                MemberNames = validationResult.MemberNames;
            }

            [DataMember]
            public string ErrorMessage {get;set;}

            [DataMember]
            public IEnumerable<string> MemberNames {get;set;}
        }

        #endregion
    }
}
