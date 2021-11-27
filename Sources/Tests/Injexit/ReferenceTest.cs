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
        public void ShouldResolveReferenceToLocallyCreatedBindingId()
        {
            var bar = new Bar();

            var barId = _fixture.BindInstance(bar)
                                .Id();
            _fixture.BindReference<IBar>(barId);

            var bar2 = _fixture.Resolve<IBar>();

            Assert.That(bar2, Is.SameAs(bar));
        }

        [Test]
        public void ShouldResolveReferenceToBindingIdDefinedOutsideUsingScope()
        {
            var bar = new Bar();
            var barId = new BindingId();

            _fixture.BindInstance(bar)
                    .Id(barId);
            _fixture.BindToSelf<Foo>()
                    .Using(x => x.BindReference<IBar>(barId));

            var foo = _fixture.Resolve<Foo>();

            Assert.That(foo.Bar, Is.SameAs(bar));
        }

        [Test]
        public void ShouldThrowIfBindingIdDoesNotImplementReferenceAbstractionType()
        {
            var fooId = _fixture.BindAnonymous<Foo>()
                                .Id();
            _fixture.BindReference<IBar>(fooId);

            var exception = Assert.Throws<DependencyException>(() => _fixture.Resolve<IBar>());

            Assert.That(
                exception.Message,
                Does.StartWith("BindingId concrete type Foo is not assignable to Reference abstraction type IBar"));
        }

        [Test]
        public void ShouldThrowIfBindingReferenceToUnboundId()
        {
            var fooId = new BindingId();
            _fixture.BindReference<IBar>(fooId);

            var exception = Assert.Throws<DependencyException>(() => _fixture.Resolve<IBar>());

            Assert.That(
                exception.Message,
                Does.StartWith(
                    "No binding associated with Id for abstraction type IBar. You are missing a call to .Id(...)"));
        }
    }
}