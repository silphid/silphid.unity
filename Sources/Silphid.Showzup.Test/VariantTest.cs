using System.Linq;
using NUnit.Framework;

namespace Silphid.Showzup.Test
{
    [TestFixture]
    public class VariantTest
    {
        [Test]
        public void Test_1_variant_without_category()
        {
            var fixture = Variant.Parse("iOS").Single();

            AssertVariant(fixture, null, "iOS");
        }

        [Test]
        public void Test_1_variant_with_category()
        {
            var fixture = Variant.Parse("Platform/iOS").Single();

            AssertVariant(fixture, "Platform", "iOS");
        }

        [Test]
        public void Test_3_variants_in_1_category()
        {
            var fixture = Variant.Parse("Platform/iOS+tvOS+Android").ToArray();

            Assert.That(fixture.Length, Is.EqualTo(3));
            AssertVariant(fixture[0], "Platform", "iOS");
            AssertVariant(fixture[1], "Platform", "tvOS");
            AssertVariant(fixture[2], "Platform", "Android");
        }

        [Test]
        public void Test_3_variants_without_category()
        {
            var fixture = Variant.Parse("iOS, tvOS, Android").ToArray();

            Assert.That(fixture.Length, Is.EqualTo(3));
            AssertVariant(fixture[0], null, "iOS");
            AssertVariant(fixture[1], null, "tvOS");
            AssertVariant(fixture[2], null, "Android");
        }

        [Test]
        public void Test_4_variants_in_2_categories()
        {
            var fixture = Variant.Parse("Platform/iOS+tvOS, Form/Mobile+Tablet").ToArray();

            Assert.That(fixture.Length, Is.EqualTo(4));
            AssertVariant(fixture[0], "Platform", "iOS");
            AssertVariant(fixture[1], "Platform", "tvOS");
            AssertVariant(fixture[2], "Form", "Mobile");
            AssertVariant(fixture[3], "Form", "Tablet");
        }

        private void AssertVariant(Variant fixture, string category, string name)
        {
            Assert.That(fixture.Category, Is.EqualTo(category));
            Assert.That(fixture.Name, Is.EqualTo(name));

        }
    }
}