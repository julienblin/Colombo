using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace Colombo
{
    [DataContract]
    public abstract class ValidatedResponse : Response, IDeserializationCallback
    {
        private BindingList<ValidationResult> validationResults;
        [IgnoreDataMember]
        public virtual BindingList<ValidationResult> ValidationResults
        {
            get
            {
                if (validationResults == null)
                {
                    validationResults = new BindingList<ValidationResult>();
                    validationResults.ListChanged += new ListChangedEventHandler(validationResults_ListChanged);
                }
                return validationResults;
            }
        }

        [DataMember]
        private List<> validationResultsSerialization;
        
        void validationResults_ListChanged(object sender, ListChangedEventArgs e)
        {
            if (validationResultsSerialization == null)
                validationResultsSerialization = new List<string>();
            else
                validationResultsSerialization.Clear();

            foreach (var validationResult in ValidationResults)
            {
                errorMessages.Add(msg.ErrorMessage);
            }
        }

        public virtual bool IsValid()
        {
            return (ValidationResults.Count == 0);
        }

        public void OnDeserialization(object sender)
        {
            ValidationResults.RaiseListChangedEvents = false;
            foreach (var msg in errorMessages)
            {
                ValidationResults.Add(new ValidationResult(msg));
            }
            ValidationResults.RaiseListChangedEvents = true;
        }

        private class SerializableValidationResult
        {
            public string 
        }
    }
}
