using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;

namespace Colombo.Interceptors
{
    /// <summary>
    /// Interceptor that performs Data Annotations validations before requests are sent.
    /// The validation will fill the <see cref="ValidatedResponse.ValidationResults"/> property of a <see cref="ValidatedResponse"/>
    /// and returns it without sending if validation errors are found.
    /// It will throw a <see cref="ValidationException"/> if the response associated with the request is not assignable to <see cref="ValidatedResponse"/>.
    /// </summary>
    /// <seealso cref="DataAnnotationsValidationHandleInterceptor"/>
    public class DataAnnotationsValidationSendInterceptor : IMessageBusSendInterceptor
    {
        /// <summary>
        /// Performs the validations
        /// </summary>
        public void Intercept(IColomboSendInvocation nextInvocation)
        {
            if (nextInvocation == null) throw new ArgumentNullException("nextInvocation");
            Contract.EndContractBlock();

            var requestsToDelete = new List<BaseRequest>();
            var responsesGroup = new ResponsesGroup();
            foreach (var request in nextInvocation.Requests)
            {
                if ((typeof(ValidatedResponse).IsAssignableFrom(request.GetResponseType())))
                {
                    var validationResults = new List<ValidationResult>();
                    if (!Validator.TryValidateObject(request, new ValidationContext(request, null, null), validationResults, true))
                    {
                        var response = (ValidatedResponse)request.CreateResponse();
                        foreach (var validationResult in validationResults)
                            response.ValidationResults.Add(validationResult);

                        responsesGroup[request] = response;
                        requestsToDelete.Add(request);
                    }
                }
                else
                {
                    Validator.ValidateObject(request, new ValidationContext(request, null, null), true);
                }
            }

            foreach (var request in requestsToDelete)
                nextInvocation.Requests.Remove(request);

            nextInvocation.Proceed();

            if (responsesGroup.Count <= 0) return;

            if (nextInvocation.Responses == null)
                nextInvocation.Responses = new ResponsesGroup();

            foreach (var item in responsesGroup)
            {
                nextInvocation.Responses.Add(item.Key, item.Value);
            }
        }

        /// <summary>
        /// Low.
        /// </summary>
        public int InterceptionPriority
        {
            get { return InterceptionPrority.Low; }
        }
    }
}
