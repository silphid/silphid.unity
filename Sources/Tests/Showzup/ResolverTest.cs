using System;
using System.Collections.Generic;
using NUnit.Framework;
using Silphid.Showzup.Resolving;

namespace Silphid.Showzup.Test
{
    [TestFixture]
    public class ResolverTest
    {
        #region DummyManifest class

        public class DummyManifest : IManifest
        {
            public List<TypeToTypeMapping> ModelsToViewModels { get; } = new List<TypeToTypeMapping>();
            public List<TypeToTypeMapping> ViewModelsToViews { get; } = new List<TypeToTypeMapping>();
            public List<ViewToPrefabMapping> ViewsToPrefabs { get; } = new List<ViewToPrefabMapping>();
        }

        #endregion

        #region Variants

        public class Temper : Variant<Temper>
        {
            public static readonly Temper Good = new Temper(nameof(Good));
            public static readonly Temper Bad = new Temper(nameof(Bad));

            public Temper(string name)
                : base(name) {}
        }

        public class Speed : Variant<Speed>
        {
            public static readonly Speed Slow = Add(nameof(Slow));
            public static readonly Speed Fast = Add(nameof(Fast));

            protected static Speed Add(string name) =>
                new Speed(name);

            public Speed(string name)
                : base(name) {}
        }

        private readonly VariantSet Empty = VariantSet.Empty;
        private readonly VariantSet Fast = new VariantSet(Speed.Fast);

        #endregion

        #region MVVM classes

        private class Animal {}

        private class Dog : Animal {}

        private class AnimalViewModel : ViewModel<Animal>
        {
            public AnimalViewModel(Animal model)
                : base(model) {}
        }

        private class DogViewModel : ViewModel<Dog>
        {
            public DogViewModel(Dog model)
                : base(model) {}
        }

        private class AnimalView : View<AnimalViewModel> {}

        private class DogView : View<DogViewModel> {}

        private class InnerViewModel : IViewModel
        {
            public object Model => null;
        }

        private class OuterViewModel : ViewModel<InnerViewModel>
        {
            public OuterViewModel(InnerViewModel model)
                : base(model) {}
        }

        private class OuterView : View<OuterViewModel> {}

        #endregion

        private IManifest _manifest;
        private Resolver _fixture;

        private static readonly Uri AnimalPrefabUri = new Uri("res://Animal");
        private static readonly Uri DogPrefabUri = new Uri("res://Dog");
        private static readonly Uri OuterPrefabUri = new Uri("res://Outer");
        private readonly TypeModelCollection _typeModelCollection = new TypeModelCollection();

        [SetUp]
        public void SetUp()
        {
            _manifest = new DummyManifest();

            // Animal -(Fast)> AnimalViewModel -(Fast)> AnimalView -[Fast](Fast)> AnimalPrefabUri
            _manifest.ModelsToViewModels.Add(CreateMapping<Animal, AnimalViewModel>(Empty, Fast));
            _manifest.ViewModelsToViews.Add(CreateMapping<AnimalViewModel, AnimalView>(Empty, Fast));
            _manifest.ViewsToPrefabs.Add(CreateMapping<AnimalView>(AnimalPrefabUri, Fast));

            // Dog -> DogViewModel -> DogView -> DogPrefabUri
            _manifest.ModelsToViewModels.Add(CreateMapping<Dog, DogViewModel>(Empty, Empty));
            _manifest.ViewModelsToViews.Add(CreateMapping<DogViewModel, DogView>(Empty, Empty));
            _manifest.ViewsToPrefabs.Add(CreateMapping<DogView>(DogPrefabUri, Empty));

            // InnerViewModel -> OuterViewModel -> OuterView -> OuterPrefabUri
            _manifest.ModelsToViewModels.Add(CreateMapping<InnerViewModel, OuterViewModel>(Empty, Empty));
            _manifest.ViewModelsToViews.Add(CreateMapping<OuterViewModel, OuterView>(Empty, Empty));
            _manifest.ViewsToPrefabs.Add(CreateMapping<OuterView>(OuterPrefabUri, Empty));

            _fixture = new Resolver(_manifest, new ScoreEvaluator(), _typeModelCollection);
        }

        [Test]
        public void RequestingFastDog_ResolveToFastAnimalView()
        {
            var problem = new Problem(_typeModelCollection.GetModelFromType(typeof(Dog)), new VariantSet(Fast));

            var info = _fixture.Resolve(problem);

            Assert.That(info.Model, Is.EqualTo(_typeModelCollection.GetModelFromType(typeof(Animal))));
            Assert.That(info.ViewModel, Is.EqualTo(_typeModelCollection.GetModelFromType(typeof(AnimalViewModel))));
            Assert.That(info.View, Is.EqualTo(_typeModelCollection.GetModelFromType(typeof(AnimalView))));
            Assert.That(info.Prefab, Is.EqualTo(AnimalPrefabUri));
        }

        [Test]
        public void RequestingDog_ResolveToDogView()
        {
            var problem = new Problem(_typeModelCollection.GetModelFromType(typeof(Dog)), new VariantSet());

            var info = _fixture.Resolve(problem);

            Assert.That(info.Model, Is.EqualTo(_typeModelCollection.GetModelFromType(typeof(Dog))));
            Assert.That(info.ViewModel, Is.EqualTo(_typeModelCollection.GetModelFromType(typeof(DogViewModel))));
            Assert.That(info.View, Is.EqualTo(_typeModelCollection.GetModelFromType(typeof(DogView))));
            Assert.That(info.Prefab, Is.EqualTo(DogPrefabUri));
        }

        [Test]
        public void RequestingInnerViewModel_ResolveToOuterView()
        {
            var problem = new Problem(
                _typeModelCollection.GetModelFromType(typeof(InnerViewModel)),
                new VariantSet());

            var info = _fixture.Resolve(problem);

            Assert.That(info.Model, Is.EqualTo(_typeModelCollection.GetModelFromType(typeof(InnerViewModel))));
            Assert.That(info.ViewModel, Is.EqualTo(_typeModelCollection.GetModelFromType(typeof(OuterViewModel))));
            Assert.That(info.View, Is.EqualTo(_typeModelCollection.GetModelFromType(typeof(OuterView))));
            Assert.That(info.Prefab, Is.EqualTo(OuterPrefabUri));
        }

        private TypeToTypeMapping CreateMapping<T, U>(VariantSet explicitVariants = null,
                                                      VariantSet implicitVariants = null) =>
            new TypeToTypeMapping(typeof(T), typeof(U), explicitVariants ?? Empty)
            {
                ImplicitVariants = implicitVariants ?? Empty
            };

        private ViewToPrefabMapping CreateMapping<T>(Uri prefabUri, VariantSet explicitVariants = null) =>
            new ViewToPrefabMapping(typeof(T), prefabUri, null, explicitVariants ?? Empty);
    }
}