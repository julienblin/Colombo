using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using Castle.Core.Logging;

namespace Colombo.Interceptors
{
    public class RequiredInContextHandlerInterceptor : IRequestHandlerInterceptor
    {
        private ILogger logger = NullLogger.Instance;
        public ILogger Logger
        {
            get { return logger; }
            set { logger = value; }
        }

        public void Intercept(IColomboSingleInvocation invocation)
        {
            if (invocation == null) throw new ArgumentNullException("invocation");
            Contract.EndContractBlock();

            RequiredInContextAttribute[] reqAttributes = invocation.Request.GetCustomAttributes<RequiredInContextAttribute>(true);
            if (reqAttributes.Length > 0)
            {
                var keys = reqAttributes.SelectMany(x => x.GetKeys()).Distinct();
                var missingKeys = new List<string>();
                foreach (var key in keys)
                {
                    if (   !invocation.Request.Context.Keys.Contains(key)
                        || string.IsNullOrEmpty(invocation.Request.Context[key])
                       )
                        missingKeys.Add(key);
                }

                if (missingKeys.Count > 0)
                    LogAndThrowError("Missing keys in Context: {0}", string.Join(", ", missingKeys));
            }

            invocation.Proceed();
        }

        public int InterceptionPriority
        {
            get { return InterceptorPrority.Medium; }
        }

        private void LogAndThrowError(string format, params object[] args)
        {
            if (format == null) throw new ArgumentNullException("format");
            if (args == null) throw new ArgumentNullException("args");
            Contract.EndContractBlock();

            var errorMessage = string.Format(format, args);
            Logger.Error(errorMessage);
            throw new RequiredInContextException(errorMessage);
        }
    }
}
