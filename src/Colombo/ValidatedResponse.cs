#region License
// The MIT License
// 
// Copyright (c) 2011 Julien Blin, julien.blin@gmail.com
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
#endregion

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
