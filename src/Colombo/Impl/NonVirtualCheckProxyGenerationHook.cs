using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Castle.DynamicProxy;
using System.Reflection;

namespace Colombo.Impl
{
    public class NonVirtualCheckProxyGenerationHook : IProxyGenerationHook
    {
        public void MethodsInspected()
        {
            // no-op
        }

        public void NonProxyableMemberNotification(Type type, MemberInfo memberInfo)
        {
            switch (memberInfo.DeclaringType.FullName)
            {
                case "System.Object":
                case "Colombo.Response":
                case "Colombo.BaseMessage":
                    break;
                default:
                    throw new ColomboException(
                        string.Format("Unable to FutureSend() for response type {0}: the member {1} is non-virtual. To enable FutureSend functionnality, all the members of the response type {0} must be marked virtual",
                            type.FullName,
                            memberInfo.Name
                        )
                    );
            }
        }

        public bool ShouldInterceptMethod(Type type, System.Reflection.MethodInfo methodInfo)
        {
            return true;
        }
    }
}
