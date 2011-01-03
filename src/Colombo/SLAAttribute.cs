using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Colombo
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=false)]
    public class SLAAttribute : Attribute
    {
        public SLAAttribute(int allowedMilliseconds) : this(new TimeSpan(0, 0, 0, 0, allowedMilliseconds)) { }

        public SLAAttribute(TimeSpan allowed)
        {
            Allowed = allowed;
        }

        public TimeSpan Allowed { get; private set; }
    }
}
