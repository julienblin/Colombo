using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Colombo.TestSupport;
using NUnit.Framework;

namespace Colombo.Samples.Messages.Tests
{
    [TestFixture]
    public class MessagesTests
    {
        [Test]
        public void All_messages_should_conforms_to_Colombo()
        {
            ColomboTest.AssertThat.AllMessagesAreConformInAssemblyThatContains<HelloWorldRequest>();
        }
    }
}
