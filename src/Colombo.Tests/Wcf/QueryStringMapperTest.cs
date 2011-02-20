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
using System.Collections.Generic;
using Colombo.Wcf;
using NUnit.Framework;

namespace Colombo.Tests.Wcf
{
    [TestFixture]
    public class QueryStringMapperTest
    {
        [Test]
        public void It_should_map_simple_properties()
        {
            var request = (TestSimpleRequest)QueryStringMapper.Map("Name=Foo&Num=3&Dec=24.6&Dbl=45.78&When=2011-01-01", typeof(TestSimpleRequest));

            Assert.AreEqual("Foo", request.Name);
            Assert.AreEqual(3, request.Num);
            Assert.AreEqual(24.6m, request.Dec);
            Assert.AreEqual(45.78, request.Dbl);
            Assert.AreEqual(new DateTime(2011,01,01), request.When);
        }

        [Test]
        public void It_should_support_lowercase_parameters()
        {
            var request = (TestSimpleRequest)QueryStringMapper.Map("name=Foo", typeof(TestSimpleRequest));

            Assert.AreEqual("Foo", request.Name);
        }

        [Test]
        public void It_should_map_subobjects()
        {
            var request = (TestSubObjectRequest)QueryStringMapper.Map("Me.FirstName=Foo&Me.LastName=Bar", typeof(TestSubObjectRequest));
            Assert.AreEqual("Foo", request.Me.FirstName);
            Assert.AreEqual("Bar", request.Me.LastName);
        }

        [Test]
        public void It_should_map_arrays()
        {
            var request = (TestArrayRequest)QueryStringMapper.Map("Names=Foo&Names=Bar", typeof(TestArrayRequest));
            Assert.AreEqual(2, request.Names.Length);
            Assert.That(request.Names, Contains.Item("Foo"));
            Assert.That(request.Names, Contains.Item("Bar"));

            request = (TestArrayRequest)QueryStringMapper.Map("Names[0]=Foo&Names[1]=Bar", typeof(TestArrayRequest));
            Assert.AreEqual(2, request.Names.Length);
            Assert.That(request.Names, Contains.Item("Foo"));
            Assert.That(request.Names, Contains.Item("Bar"));
        }

        [Test]
        public void It_should_map_lists_with_subobjects()
        {
            var request = (TestSubObjectListRequest)QueryStringMapper.Map("People[0].FirstName=Foo&People[0].LastName=Bar&People[1].LastName=FooBar", typeof(TestSubObjectListRequest));
            Assert.AreEqual(2, request.People.Count);
            Assert.AreEqual("Foo", request.People[0].FirstName);
            Assert.AreEqual("Bar", request.People[0].LastName);
            Assert.AreEqual("FooBar", request.People[1].LastName);
        }

        public class TestSimpleRequest : Request<TestResponse>
        {
            public string Name { get; set; }

            public int Num { get; set; }

            public decimal Dec { get; set; }

            public double Dbl { get; set; }

            public DateTime When { get; set; }
        }

        public class TestSubObjectRequest : Request<TestResponse>
        {
            public Person Me { get; set; }
        }

        public class TestSubObjectListRequest : Request<TestResponse>
        {
            public IList<Person> People { get; set; }
        }

        public class Person
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }

        public class TestArrayRequest : Request<TestResponse>
        {
            public string[] Names { get; set; }
        }
    }
}
