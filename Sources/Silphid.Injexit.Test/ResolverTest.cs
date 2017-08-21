using NUnit.Framework;

namespace Silphid.Injexit.Test
{
    [TestFixture]
    public class ResolverTest
    {
        public interface IFoo {}
        public interface IBar {}
        public class Foo : IFoo {}
        
        public class Bar_ConstructorInjection : IBar
        {
            public Bar_ConstructorInjection(IFoo foo) { }
        }

        public class Bar_MethodInjection : IBar
        {
            [Inject]
            public void Inject(IFoo foo) { }
        }

        public class Bar_FieldInjection : IBar
        {
            [Inject] public IFoo Foo;
        }
        
        private IContainer _fixture;

        [SetUp]
        public void SetUp()
        {
            _fixture = new Container(new Reflector());
        }

        [Test]
        public void MissingBindingForDependency_ConstructorInjection_ShouldThrow()
        {
            AssertExceptionThrownWhenMissingBindingForDependencyOf<Bar_ConstructorInjection>();
        }

        [Test]
        public void MissingBindingForDependency_MethodInjection_ShouldThrow()
        {
            AssertExceptionThrownWhenMissingBindingForDependencyOf<Bar_MethodInjection>();
        }

        [Test]
        public void MissingBindingForDependency_FieldInjection_ShouldThrow()
        {
            AssertExceptionThrownWhenMissingBindingForDependencyOf<Bar_FieldInjection>();
        }

        public void AssertExceptionThrownWhenMissingBindingForDependencyOf<TBar>() where TBar : IBar
        {
            _fixture.Bind<IBar, TBar>();
            
            var ex = Assert.Throws<UnresolvedTypeException>(() =>
                _fixture.Resolve<IFoo>());
            
            Assert.That(ex.Type, Is.EqualTo(typeof(IFoo)));
        }
    }
}