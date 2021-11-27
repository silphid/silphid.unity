using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable MemberHidesStaticFromOuterClass

namespace Silphid.Injexit.Test
{
    [TestFixture]
    public class ResolverTest
    {
        public interface IFoo {}

        public interface IGoo {}

        public interface IBar {}

        public class Foo : IFoo {}

        public class Bar_ConstructorInjection : IBar
        {
            public Bar_ConstructorInjection(IFoo foo) {}
        }

        public class Bar_MethodInjection : IBar
        {
            [Inject]
            public void Inject(IFoo foo) {}
        }

        public class Bar_FieldInjection : IBar
        {
            [Inject] public IFoo Foo;
        }

        private interface IBarWithFoos : IBar
        {
            IEnumerable<IFoo> Foos { get; }
        }

        private class BarWithFooList : IBarWithFoos
        {
            public IEnumerable<IFoo> Foos { get; }

            public BarWithFooList(List<IFoo> foos)
            {
                Foos = foos;
            }
        }

        private class BarWithFooArray : IBarWithFoos
        {
            public IEnumerable<IFoo> Foos { get; }

            public BarWithFooArray(IFoo[] foos)
            {
                Foos = foos;
            }
        }

        private class BarWithFooEnumerable : IBarWithFoos
        {
            public IEnumerable<IFoo> Foos { get; }

            public BarWithFooEnumerable(IEnumerable<IFoo> foos)
            {
                Foos = foos;
            }
        }

        public class GooWithBarDependency : IGoo
        {
            public GooWithBarDependency(IBar bar) {}
        }

        public class BarWithFooDependency : IBar
        {
            public BarWithFooDependency(IFoo foo) {}
        }

        public class CircularBar : IBar
        {
            public CircularBar(IFoo foo) {}
        }

        public class CircularFoo : IFoo
        {
            public CircularFoo(IBar bar) {}
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
            AssertExceptionThrownWhenMissingBindingForDependencyOf<Bar_ConstructorInjection, IFoo>();
        }

        [Test]
        public void MissingBindingForDependency_MethodInjection_ShouldThrow()
        {
            AssertExceptionThrownWhenMissingBindingForDependencyOf<Bar_MethodInjection, IFoo>();
        }

        [Test]
        public void MissingBindingForDependency_FieldInjection_ShouldThrow()
        {
            AssertExceptionThrownWhenMissingBindingForDependencyOf<Bar_FieldInjection, IFoo>();
        }

        [Test]
        public void MissingBindingForDependency_ConstructorInjection_List_ShouldThrow()
        {
            AssertExceptionThrownWhenMissingBindingForDependencyOf<BarWithFooList, List<IFoo>>();
        }

        [Test]
        public void MissingBindingForDependency_ConstructorInjection_Array_ShouldThrow()
        {
            AssertExceptionThrownWhenMissingBindingForDependencyOf<BarWithFooArray, IFoo[]>();
        }

        [Test]
        public void MissingBindingForDependency_ConstructorInjection_Enumerable_ShouldThrow()
        {
            AssertExceptionThrownWhenMissingBindingForDependencyOf<BarWithFooEnumerable, IEnumerable<IFoo>>();
        }

        private void AssertExceptionThrownWhenMissingBindingForDependencyOf<TDependent, TDependency>()
            where TDependent : IBar
        {
            _fixture.Bind<IBar, TDependent>();

            var ex = Assert.Throws<DependencyException>(() => _fixture.Resolve<IBar>());

            Assert.That(ex.DependentTypes.Last(), Is.EqualTo(typeof(TDependent)));
            Assert.That(ex.Type, Is.EqualTo(typeof(TDependency)));
        }

        [Test]
        public void MissingDependencyAtThirdLevel_ShouldThrow_ReportDependentTypeOfSecondLevel()
        {
            _fixture.Bind<IGoo, GooWithBarDependency>();
            _fixture.Bind<IBar, BarWithFooDependency>();

            var ex = Assert.Throws<DependencyException>(() => _fixture.Resolve<IGoo>());

            Assert.That(
                ex.DependentTypes,
                Is.EqualTo(new[] { typeof(GooWithBarDependency), typeof(BarWithFooDependency) }));
            Assert.That(ex.Type, Is.EqualTo(typeof(IFoo)));
        }

        [Test]
        public void ShouldResolveListDependencies_List()
        {
            AssertCanResolveListDependencyOf<BarWithFooList, List<IFoo>>();
        }

        [Test]
        public void ShouldResolveListDependencies_Array()
        {
            AssertCanResolveListDependencyOf<BarWithFooArray, IFoo[]>();
        }

        [Test]
        public void ShouldResolveListDependencies_Enumerable()
        {
            AssertCanResolveListDependencyOf<BarWithFooEnumerable, IFoo[]>();
        }

        [Test]
        public void ShouldThrowWhenDetectingCircularDependencies()
        {
            _fixture.Bind<IBar, CircularBar>();
            _fixture.Bind<IFoo, CircularFoo>();

            var exception = Assert.Throws<CircularDependencyException>(() => _fixture.Resolve<IFoo>());

            Assert.That(
                exception.Message,
                Is.EqualTo("Circular dependency detected: CircularFoo > CircularBar > CircularFoo"));
        }

        private void AssertCanResolveListDependencyOf<TBarWithFoos, TList>() where TBarWithFoos : IBarWithFoos
        {
            _fixture.Bind<IBarWithFoos, TBarWithFoos>();
            _fixture.BindList<IFoo>(
                x =>
                {
                    x.Add<Foo>();
                    x.Add<Foo>();
                    x.Add<Foo>();
                });

            var bar = _fixture.Resolve<IBarWithFoos>();

            Assert.That(bar, Is.TypeOf<TBarWithFoos>());
            Assert.That(bar.Foos, Is.TypeOf<TList>());
            Assert.That(bar.Foos.Count(), Is.EqualTo(3));
            Assert.That(bar.Foos.First(), Is.TypeOf<Foo>());
        }
    }
}