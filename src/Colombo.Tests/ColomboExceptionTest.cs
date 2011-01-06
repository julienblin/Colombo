using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace Colombo.Tests
{
    [TestFixture]
    public class ColomboExceptionTest : BaseTest
    {
        [Test]
        public void It_should_display_inner_exception_in_ToString()
        {
            try
            {
                var testEx = new TestException();
                throw new ColomboException("ColomboExceptionMessage", testEx);
            }
            catch (ColomboException ex)
            {
                Assert.That(() => ex.ToString(),
                    Contains.Substring("TestExceptionContent"));
            }
        }

        [Test]
        public void It_should_display_inner_exception_for_aggregate_exception_in_ToString()
        {
            try
            {
                var testEx = new TestException();
                var testEx2 = new TestException2();
                var aggregateException = new AggregateException(testEx, testEx2);
                throw new ColomboException("ColomboExceptionMessage", aggregateException);
            }
            catch (ColomboException ex)
            {
                Console.WriteLine(ex);
                Assert.That(() => ex.ToString(),
                    Contains.Substring("TestExceptionContent")
                    .And.Contains("TestException2Content"));
            }
        }

        public class TestException : Exception
        {
            public TestException()
                : base("TestExceptionContent")
            {
            }
        }

        public class TestException2 : Exception
        {
            public TestException2()
                : base("TestException2Content")
            {
            }
        }
    }
}
