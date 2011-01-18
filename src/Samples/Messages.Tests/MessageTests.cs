using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Colombo.Samples.Messages;
using Colombo.TestSupport;
using NUnit.Framework;

namespace Messages.Tests
{
    [TestFixture]
    public class MessageTests
    {
        [Test]
        public void Messages_should_be_compatible_with_Colombo()
        {
            ColomboTest.AssertThat.AllMessagesAreConformInAssemblyThatContains<HelloWorldRequest>();
        }
    }
}
