using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Colombo.Host.Tests
{
    [TestFixture]
    public class ProgramTest
    {
        [Test]
        public void It_should_display_help_and_exit_with_0()
        {
            Assert.That(Program.Main(new[] { "/h" }), Is.EqualTo(0));
        }
    }
}
