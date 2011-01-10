﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.ComponentModel.DataAnnotations;

namespace Colombo.Interceptors
{
    public class DataAnnotationsValidationHandleInterceptor : IRequestHandlerHandleInterceptor
    {
        public void Intercept(IColomboHandleInvocation nextInvocation)
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

        public int InterceptionPriority
        {
            get { return InterceptorPrority.Low; }
        }
    }
}