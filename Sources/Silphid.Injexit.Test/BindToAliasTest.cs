using System.Collections.Generic;
using JetBrains.Annotations;
using NUnit.Framework;

namespace Silphid.Injexit.Test
{
    [TestFixture]
    public class BindToAliasTest
    {
        private interface IFoo {}
        private interface IBar {}
        
        [UsedImplicitly]
        private class Foo : IFoo, IBar {}
        
        [UsedImplicitly]
        private class Bar : IFoo, IBar {}
        
        [Test]
        public void SameBindingShouldBeUsedForBothInterfaces()
        {
            var container = new Container(new Reflector());

            container.Bind<IFoo, Foo>().AsSingle().AsAlias("foo");
            container.BindToAlias<IBar>("foo");

            var actualFoo = container.Resolve<IFoo>();
            var actualBar = container.Resolve<IBar>();

            Assert.That(actualFoo, Is.TypeOf<Foo>());
            Assert.That(actualFoo, Is.SameAs(actualBar));
        }
        
        [Test]
        public void ItemInListCanBeReusedAsAlias()
        {
            var container = new Container(new Reflector());

            container.BindList<IFoo>(x =>
                x.Add<Foo>().AsSingle().AsAlias("foo"));
            
            container.BindToAlias<IBar>("foo");

            var fooInsideList = container.Resolve<List<IFoo>>()[0];
            var barOutsideList = container.Resolve<IBar>();

            Assert.That(fooInsideList, Is.SameAs(barOutsideList));
        }
    }
}