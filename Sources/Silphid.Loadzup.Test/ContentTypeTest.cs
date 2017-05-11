using System;
using NUnit.Framework;
// ReSharper disable ObjectCreationAsStatement

namespace Silphid.Loadzup.Test
{
    [TestFixture]
    public class ContentTypeTest
    {
        [Test]
        public void Test_type_subtype_charset()
        {
            var fixture = new ContentType("text/html; charset=ISO-8859-4");

            Assert.That(fixture.Type, Is.EqualTo("text"));
            Assert.That(fixture.SubType, Is.EqualTo("html"));
            Assert.That(fixture.CharSet, Is.EqualTo("ISO-8859-4"));
            Assert.That(fixture.Name, Is.Null);
            Assert.That(fixture.Boundary, Is.Null);
        }

        [Test]
        public void Test_type_subtype_name()
        {
            var fixture = new ContentType("text/html; name=SomeName");

            Assert.That(fixture.Type, Is.EqualTo("text"));
            Assert.That(fixture.SubType, Is.EqualTo("html"));
            Assert.That(fixture.CharSet, Is.Null);
            Assert.That(fixture.Name, Is.EqualTo("SomeName"));
            Assert.That(fixture.Boundary, Is.Null);
        }

        [Test]
        public void Test_type_subtype_boundary()
        {
            var fixture = new ContentType("text/html; boundary=SomeBoundary");

            Assert.That(fixture.Type, Is.EqualTo("text"));
            Assert.That(fixture.SubType, Is.EqualTo("html"));
            Assert.That(fixture.CharSet, Is.Null);
            Assert.That(fixture.Name, Is.Null);
            Assert.That(fixture.Boundary, Is.EqualTo("SomeBoundary"));
        }

        [Test]
        public void Test_type_subtype_charset_name_boundary()
        {
            var fixture = new ContentType("text/html; charset=ISO-8859-4; name=SomeName; boundary=SomeBoundary");

            Assert.That(fixture.Type, Is.EqualTo("text"));
            Assert.That(fixture.SubType, Is.EqualTo("html"));
            Assert.That(fixture.CharSet, Is.EqualTo("ISO-8859-4"));
            Assert.That(fixture.Name, Is.EqualTo("SomeName"));
            Assert.That(fixture.Boundary, Is.EqualTo("SomeBoundary"));
        }

        [Test]
        public void Test_type_subtype()
        {
            var fixture = new ContentType("text/html");

            Assert.That(fixture.Type, Is.EqualTo("text"));
            Assert.That(fixture.SubType, Is.EqualTo("html"));
            Assert.That(fixture.CharSet, Is.Null);
            Assert.That(fixture.Name, Is.Null);
            Assert.That(fixture.Boundary, Is.Null);
        }

        [Test]
        public void Test_trailing_semi_colon()
        {
            var fixture = new ContentType("text/html; ");

            Assert.That(fixture.Type, Is.EqualTo("text"));
            Assert.That(fixture.SubType, Is.EqualTo("html"));
            Assert.That(fixture.CharSet, Is.Null);
            Assert.That(fixture.Name, Is.Null);
            Assert.That(fixture.Boundary, Is.Null);
        }

        [Test]
        public void Test_missing_type()
        {
            Assert.Throws<FormatException>(() => new ContentType("/html; charset=ISO-8859-4"));
        }

        [Test]
        public void Test_missing_subtype()
        {
            Assert.Throws<FormatException>(() => new ContentType("text/; charset=ISO-8859-4"));
        }

        [Test]
        public void Test_missing_parameter_name()
        {
            Assert.Throws<FormatException>(() => new ContentType("text/html; =ISO-8859-4"));
        }

        [Test]
        public void Test_missing_parameter_value()
        {
            Assert.Throws<FormatException>(() => new ContentType("text/html; charset="));
        }

        [Test]
        public void Test_missing_parameter_assignment()
        {
            Assert.Throws<FormatException>(() => new ContentType("text/html; charset"));
        }
    }
}