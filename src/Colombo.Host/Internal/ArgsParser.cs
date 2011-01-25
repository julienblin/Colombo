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
