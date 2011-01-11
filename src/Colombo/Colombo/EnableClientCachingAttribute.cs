using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Colombo
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=false)]
    public class EnableClientCachingAttribute : Attribute
    {
        public int Hours { get; set; }
        public int Minutes { get; set; }
        public int Seconds { get; set; }

        public TimeSpan Duration
        {
            get
            {
                if (Hours == 0 && Minutes == 0 && Seconds == 0)
                {
                    throw new ColomboException("You need to specify at least an hour value, a minute value or a second value.");
                }

                return new TimeSpan(0, Hours, Minutes, Seconds);
            }
        }
    }
}
