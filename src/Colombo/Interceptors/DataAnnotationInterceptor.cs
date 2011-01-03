using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.ComponentModel.DataAnnotations;

namespace Colombo.Interceptors
{
    public abstract class DataAnnotationInterceptor : IMessageBusSendInterceptor, IRequestHandlerInterceptor
    {
        public void Intercept(IColomboInvocation invocation)
        {
            if (invocation == null) throw new ArgumentNullException("invocation");
            Contract.EndContractBlock();

            var validationResults = new List<ValidationResult>();
            if (Validator.TryValidateObject(invocation.Request, new ValidationContext(invocation.Request, null, null), validationResults, true))
            {
                invocation.Proceed();
            }
            else
            {
                var responseType = invocation.Request.GetResponseType();
                Contract.Assume(responseType != null);
                invocation.Response = (Response)Activator.CreateInstance(responseType);
                invocation.Response.CorrelationGuid = invocation.Request.CorrelationGuid;
                invocation.Response.ValidationResults = validationResults;
            }
        }

        public int InterceptionPriority
        {
            get { return InterceptorPrority.Low; }
        }
    }
}
