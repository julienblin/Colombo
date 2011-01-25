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
