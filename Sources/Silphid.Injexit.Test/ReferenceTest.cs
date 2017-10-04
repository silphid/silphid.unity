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
        public void ShouldResolveReferenceToBindingIdDefinedOutsideUsingScope()
        {
            var bar = new Bar();
            var BarId = new BindingId("BarId");
            
            _fixture.BindInstance(bar).Id(BarId);
            _fixture.BindToSelf<Foo>().Using(x =>
                x.BindReference<IBar>(BarId));

            var foo = _fixture.Resolve<Foo>();

            Assert.That(foo.Bar, Is.SameAs(bar));
        }

        [Test]
        public void ShouldThrowIfBindingIdDoesNotImplementReferenceAbstractionType()
        {
            var FooId = new BindingId("FooId");
            _fixture.BindToSelf<Foo>().Id(FooId);
            _fixture.BindReference<IBar>(FooId);

            var exception = Assert.Throws<UnresolvedTypeException>(() => _fixture.Resolve<IBar>());

            Assert.That(exception.Message, Is.EqualTo("Failed to resolve type IBar : Binding FooId concrete type Foo is not assignable to Reference abstraction type IBar"));
        }

        [Test]
        public void ShouldThrowIfBindingReferenceToUnboundId()
        {
            var FooId = new BindingId("FooId");
            _fixture.BindReference<IBar>(FooId);
            
            var exception = Assert.Throws<UnresolvedTypeException>(() => _fixture.Resolve<IBar>());

            Assert.That(exception.Message, Is.EqualTo("Failed to resolve type IBar : No binding bound to FooId"));
        }
    }
}