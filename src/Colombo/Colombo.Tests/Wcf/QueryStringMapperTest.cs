using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            var request = (TestSimpleRequest)QueryStringMapper.Map("Name=Foo&Num=3&Flt=24.6&When=2011-01-01", typeof(TestSimpleRequest));

            Assert.AreEqual("Foo", request.Name);
            Assert.AreEqual(3, request.Num);
            Assert.AreEqual(24.6, request.Flt);
            Assert.AreEqual(new DateTime(2011,01,01), request.When);
        }

        [Test]
        public void It_should_map_subobjects()
        {
            var request = (TestSubObjectRequest)QueryStringMapper.Map("Me.FirstName=Foo&Me.LastName=Bar", typeof(TestSubObjectRequest));
            Assert.AreEqual("Foo", request.Me.FirstName);
            Assert.AreEqual("Bar", request.Me.LastName);
        }

        public class TestSimpleRequest : Request<TestResponse>
        {
            public string Name { get; set; }

            public int Num { get; set; }

            public double Flt { get; set; }

            public DateTime When { get; set; }
        }

        public class TestSubObjectRequest : Request<TestResponse>
        {
            public Person Me { get; set; }
        }

        public class Person
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
        }
    }
}
