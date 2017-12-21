using NUnit.Framework;
// ReSharper disable MemberHidesStaticFromOuterClass

namespace Silphid.Injexit.Test
{
    [TestFixture]
    public class ReferenceTest
    {
        public interface IFoo {}
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
        public void ShouldResolveReferenceToLocallyCreatedBindingId()
        {
            var bar = new Bar();

            var BarId = _fixture.BindInstance(bar).Id();
            _fixture.BindReference<IBar>(BarId);

            var bar2 = _fixture.Resolve<IBar>();

            Assert.That(bar2, Is.SameAs(bar));
        }

        [Test]
        public void ShouldResolveReferenceToBindingIdDefinedOutsideUsingScope()
        {
            var bar = new Bar();
            var BarId = new BindingId();
            
            _fixture.BindInstance(bar).Id(BarId);
            _fixture.BindToSelf<Foo>().Using(x =>
                x.BindReference<IBar>(BarId));

            var foo = _fixture.Resolve<Foo>();

            Assert.That(foo.Bar, Is.SameAs(bar));
        }

        [Test]
        public void ShouldThrowIfBindingIdDoesNotImplementReferenceAbstractionType()
        {
            var FooId = new BindingId();
            _fixture.BindToSelf<Foo>().Id(FooId);
            _fixture.BindReference<IBar>(FooId);

            var exception = Assert.Throws<DependencyException>(() => _fixture.Resolve<IBar>());

            Assert.That(exception.Message, Does.StartWith("Binding FooId concrete type Foo is not assignable to Reference abstraction type IBar"));
        }

        [Test]
        public void ShouldThrowIfBindingReferenceToUnboundId()
        {
            var FooId = new BindingId();
            _fixture.BindReference<IBar>(FooId);
            
            var exception = Assert.Throws<DependencyException>(() => _fixture.Resolve<IBar>());

            Assert.That(exception.Message, Does.StartWith("No binding bound to FooId"));
        }
    }
}