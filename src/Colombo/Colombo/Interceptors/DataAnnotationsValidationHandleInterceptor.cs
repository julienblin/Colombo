using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;

namespace Colombo.Interceptors
{
    /// <summary>
    /// Interceptor that validates the request handled based on Data annotations.
    /// The validation will fill the <see cref="ValidatedResponse.ValidationResults"/> property of a <see cref="ValidatedResponse"/>
    /// and returns it without invoking the handler if validation errors are found.
    /// It will throw a <see cref="ValidationException"/> if the response associated with the request is not assignable to <see cref="ValidatedResponse"/>.
    /// </summary>
    /// <seealso cref="DataAnnotationsValidationSendInterceptor"/>
    public class DataAnnotationsValidationHandleInterceptor : IRequestHandlerHandleInterceptor
    {
        /// <summary>
        /// Performs the validations
        /// </summary>
        public void Intercept(IColomboRequestHandleInvocation nextInvocation)
        {
            if (nextInvocation == null) throw new ArgumentNullException("nextInvocation");
            Contract.EndContractBlock();

            if ((typeof(ValidatedResponse).IsAssignableFrom(nextInvocation.Request.GetResponseType())))
            {
                var validationResults = new List<ValidationResult>();
                if (Validator.TryValidateObject(nextInvocation.Request, new ValidationContext(nextInvocation.Request, null, null), validationResults, true))
                {
                    nextInvocation.Proceed();
                }
                else
                {
                    var response = (ValidatedResponse)nextInvocation.Request.CreateResponse();
                    foreach (var validationResult in validationResults)
                        response.ValidationResults.Add(validationResult);

                    nextInvocation.Response = response;
                }
            }
            else
            {
                Validator.ValidateObject(nextInvocation.Request, new ValidationContext(nextInvocation.Request, null, null), true);
                nextInvocation.Proceed();
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
