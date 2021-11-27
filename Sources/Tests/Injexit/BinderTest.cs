using System;
using NUnit.Framework;

namespace Silphid.Injexit.Test
{
    [TestFixture]
    public class BinderTest
    {
        private interface IFoo {}

        [Test]
        public void BindingInstanceToNullReference_Throws()
        {
            var container = new Container(new Reflector());

            Assert.Throws<ArgumentNullException>(() => container.BindInstance<IFoo>(null));
        }
    }
}