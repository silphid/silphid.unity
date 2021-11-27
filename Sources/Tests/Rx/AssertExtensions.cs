using NUnit.Framework;
using _Is = NUnit.Framework.Is;

namespace Silphid.Tests
{
    public static class AssertExtensions
    {
        public static void IsTrue(this bool This) =>
            Assert.That(This, _Is.True);

        public static void IsFalse(this bool This) =>
            Assert.That(This, _Is.False);

        public static void IsSameReferenceAs(this object This, object expected) =>
            Assert.That(This, _Is.SameAs(expected));

        public static void Is(this object This, object expected, string message = null) =>
            Assert.That(This, _Is.EqualTo(expected), message);

        public static void IsNull(this object This) =>
            Assert.That(This, _Is.Null);

        public static void IsInstanceOf<T>(this object This) =>
            Assert.That(This, _Is.InstanceOf<T>());
    }
}