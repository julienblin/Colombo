using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Castle.Core.Logging;

namespace Colombo.Interceptors
{
    public class RequiredInContextHandleInterceptor : IRequestHandlerHandleInterceptor
    {
        private ILogger logger = NullLogger.Instance;
        public ILogger Logger
        {
            get { return logger; }
            set { logger = value; }
        }

        public void Intercept(IColomboRequestHandleInvocation nextInvocation)
        {
            if (nextInvocation == null) throw new ArgumentNullException("nextInvocation");
            Contract.EndContractBlock();

            var reqAttributes = nextInvocation.Request.GetCustomAttributes<RequiredInContextAttribute>(true);
            if (reqAttributes.Length > 0)
            {
                var keys = reqAttributes.SelectMany(x => x.GetKeys()).Distinct();
                var missingKeys = keys.Where(key => !nextInvocation.Request.Context.Keys.Contains(key) || string.IsNullOrEmpty(nextInvocation.Request.Context[key])).ToList();

                if (missingKeys.Count > 0)
                    LogAndThrowError("Missing keys in Context: {0}", string.Join(", ", missingKeys));
            }

            nextInvocation.Proceed();
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
