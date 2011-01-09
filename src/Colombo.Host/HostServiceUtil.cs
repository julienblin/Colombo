using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

using Topshelf;

namespace Colombo.Host
{
    public static class HostServiceUtil
    {
        public static bool IsRunningInCommandLineMode()
        {
            return !IsRunningInWindowsServiceMode();
        }

        public static bool IsRunningInWindowsServiceMode()
        {
            return Process.GetCurrentProcess().GetParent().ProcessName.Equals("services");
        }
    }
}
