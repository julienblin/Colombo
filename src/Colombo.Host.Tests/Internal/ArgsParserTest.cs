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
using System.Linq.Expressions;
using Colombo.Host.Internal;
using NUnit.Framework;

namespace Colombo.Host.Tests.Internal
{
    [TestFixture]
    public class ArgsParserTest
    {
        [Test]
        public void It_should_parse_help()
        {
            TestParser(true, p => p.Help, "-h");
            TestParser(true, p => p.Help, "/h");
            TestParser(true, p => p.Help, "-help");
            TestParser(true, p => p.Help, "/?");
            TestParser(false, p => p.Help, "foo");
        }

        [Test]
        public void It_should_parse_serviceName()
        {
            TestParser<string>(null, p => p.ServiceName, "/h");
            TestParser("FooServiceName", p => p.ServiceName, "/serviceName:FooServiceName");
            TestParser("FooServiceName", p => p.ServiceName, "-serviceName=FooServiceName");
        }

        [Test]
        public void It_should_parse_displayName()
        {
            TestParser<string>(null, p => p.DisplayName, "/h");
            TestParser("FooDisplayName", p => p.DisplayName, "/displayName:FooDisplayName");
            TestParser("FooDisplayName", p => p.DisplayName, "-displayName=FooDisplayName");
        }

        [Test]
        public void It_should_parse_description()
        {
            TestParser<string>(null, p => p.Description, "/h");
            TestParser("FooDescription", p => p.Description, "/description:FooDescription");
            TestParser("FooDescription", p => p.Description, "-description=FooDescription");
        }

        [Test]
        public void It_should_parse_startManually()
        {
            TestParser(false, p => p.StartManually, "-h");
            TestParser(true, p => p.StartManually, "-startManually");
            TestParser(true, p => p.StartManually, "/startManually");
        }

        [Test]
        public void It_should_parse_username()
        {
            TestParser<string>(null, p => p.Username, "/h");
            TestParser("FooUsername", p => p.Username, "/username:FooUsername");
            TestParser("FooUsername", p => p.Username, "-username=FooUsername");
        }

        [Test]
        public void It_should_parse_password()
        {
            TestParser<string>(null, p => p.Password, "/h");
            TestParser("FooPassword", p => p.Password, "/password:FooPassword");
            TestParser("FooPassword", p => p.Password, "-password=FooPassword");
        }

        private void TestParser<T>(T expected, Expression<Func<ArgsParser, T>> property, params string[] args)
        {
            var parser = new ArgsParser();
            parser.Parse(args);
            Assert.AreEqual(expected, property.Compile().Invoke(parser));
        }
    }
}
