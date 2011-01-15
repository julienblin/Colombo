using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Contracts;

namespace Colombo.Interceptors
{
    public class DataAnnotationsValidationSendInterceptor : IMessageBusSendInterceptor
    {
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

            if (responsesGroup.Count > 0)
            {
                if (nextInvocation.Responses == null)
                    nextInvocation.Responses = new ResponsesGroup();

                foreach (var item in responsesGroup)
                {
                    nextInvocation.Responses.Add(item.Key, item.Value);
                }
            }
        }

        public int InterceptionPriority
        {
            get { return InterceptorPrority.Low; }
        }
    }
}
