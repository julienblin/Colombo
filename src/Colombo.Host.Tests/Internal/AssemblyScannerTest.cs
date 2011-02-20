using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Colombo.Host.Internal;
using NUnit.Framework;

namespace Colombo.Host.Tests.Internal
{
    [TestFixture]
    public class AssemblyScannerTest
    {
        [Test]
        public void It_should_find_Unique_configure_this_endpoint()
        {
            var baseDirectory = new DirectoryInfo(Path.GetDirectoryName(GetType().Assembly.Location));
            var assemblyScanner = new AssemblyScanner(baseDirectory);
            Assert.That(assemblyScanner.FindUniqueConfigureThisEndPoint(), Is.EqualTo(typeof (EndPointTest)));
        }
    }
}
