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
