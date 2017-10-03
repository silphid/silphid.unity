using NUnit.Framework;
// ReSharper disable MemberHidesStaticFromOuterClass

namespace Silphid.Injexit.Test
{
    [TestFixture]
    public class ReferenceTest
    {
        public interface IBar {}

        public class Foo
        {
            public IBar Bar { get; }

            public Foo(IBar bar)
            {
                Bar = bar;
            }
        }
        
        public class Bar : IBar {}

        private IContainer _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Container(new Reflector());
        }

        [Test]
        public void Test()
        {
            var bar = new Bar();

            _fixture.BindInstance<IBar>(bar).Id("Bar");
            _fixture.BindToSelf<Foo>().Using(x =>
                x.BindReference<IBar>("Bar"));

            var foo = _fixture.Resolve<Foo>();

            Assert.That(foo.Bar, Is.SameAs(bar));
        }
    }
}