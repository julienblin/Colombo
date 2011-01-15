using System.Collections.Generic;
using NDesk.Options;

namespace Colombo.Host.Internal
{
    internal class ArgsParser
    {
        internal void Parse(IEnumerable<string> args)
        {
            var p = new OptionSet {
   	            { "h|?|help",      v => Help = v != null },
                { "serviceName=",  v => ServiceName = v  },
                { "displayName=",  v => DisplayName = v  },
                { "description=",  v => Description = v  },
                { "startManually", v => StartManually = v != null },
                { "username=",     v => Username = v  },
                { "password=",     v => Password = v  }
            };
            Extra = p.Parse(args);
        }

        internal bool Help { get; private set; }

        internal string ServiceName { get; private set; }

        internal string DisplayName { get; private set; }

        internal string Description { get; private set; }

        internal bool StartManually { get; private set; }

        internal string Username { get; private set; }

        internal string Password { get; private set; }

        internal List<string> Extra { get; private set; }
    }
}
