using System;
using NUnit.Framework;

namespace Silphid.Extensions.Tests
{
    public class FloatExtensionsTests
    {
        [Test]
        public void TestRoundToInterval()
        {
            // Positives
            Assert.AreEqual(0f, 0.1f.RoundToInterval(0.25f));
            Assert.AreEqual(0.25f, 0.24f.RoundToInterval(0.25f));
            Assert.AreEqual(0.25f, 0.26f.RoundToInterval(0.25f));
            Assert.AreEqual(0.5f, 0.49f.RoundToInterval(0.25f));
            Assert.AreEqual(0.5f, 0.51f.RoundToInterval(0.25f));
            
            // Negatives
            Assert.AreEqual(0f, -0.1f.RoundToInterval(0.25f));
            Assert.AreEqual(-0.25f, -0.24f.RoundToInterval(0.25f));
            Assert.AreEqual(-0.25f, -0.26f.RoundToInterval(0.25f));
            Assert.AreEqual(-0.5f, -0.49f.RoundToInterval(0.25f));
            Assert.AreEqual(-0.5f, -0.51f.RoundToInterval(0.25f));
        }
    }
}