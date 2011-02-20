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

using System;
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
                Assert.That(ex.ToString(), Contains.Substring("TestExceptionContent"));
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
                Assert.That(ex.ToString(),
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
