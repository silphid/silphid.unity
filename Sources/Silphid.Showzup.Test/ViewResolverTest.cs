using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using NSubstitute;
using UniRx;

// ReSharper disable ClassNeverInstantiated.Local

namespace Silphid.Showzup.Test
{
    [TestFixture]
    public class ViewResolverTest
    {
        #region DummyManifest class

        public class DummyManifest : IManifest
        {
            public List<TypeToTypeMapping> ModelsToViewModels { get; } = new List<TypeToTypeMapping>();
            public List<TypeToTypeMapping> ViewModelsToViews { get; } = new List<TypeToTypeMapping>();
            public List<TypeToUriMapping> ViewsToPrefabs { get; } = new List<TypeToUriMapping>();
        }

        #endregion
        
        #region Variants

        public class Temper : Variant<Temper>
        {
            public static readonly Temper Good = new Temper();
            public static readonly Temper Bad = new Temper();
        }

        public class Speed : Variant<Speed>
        {
            public static readonly Speed Slow = new Speed();
            public static readonly Speed Fast = new Speed();
        }
        
        private readonly VariantSet Empty = VariantSet.Empty;
        private readonly VariantSet Fast = new VariantSet(Speed.Fast);

        #endregion

        #region MVVM classes

        private class Animal {}
        private class Dog : Animal {}

        private class AnimalViewModel : ViewModel<Animal>
        {
            public AnimalViewModel(Animal model) : base(model) {}
        }
        
        private class DogViewModel : ViewModel<Dog>
        {
            public DogViewModel(Dog model) : base(model) {}
        }

        private class AnimalView : View<AnimalViewModel> {}
        private class DogView : View<DogViewModel> {}

        #endregion
        
        private IManifest _manifest;
        private ViewResolver _fixture;

        private static readonly Uri AnimalPrefabUri = new Uri("res://Animal");
        private static readonly Uri DogPrefabUri = new Uri("res://Dog");

        [SetUp]
        public void SetUp()
        {
            _manifest = new DummyManifest();

            // Animal -(Fast)> AnimalViewModel -(Fast)> AnimalView -[Fast](Fast)> AnimalPrefab 
            _manifest.ModelsToViewModels.Add(CreateMapping<Animal, AnimalViewModel>(Empty, Fast));
            _manifest.ViewModelsToViews.Add(CreateMapping<AnimalViewModel, AnimalView>(Empty, Fast));
            _manifest.ViewsToPrefabs.Add(CreateMapping<AnimalView>(AnimalPrefabUri, Fast));

            // Dog -> DogViewModel -> DogView -> DogPrefab 
            _manifest.ModelsToViewModels.Add(CreateMapping<Dog, DogViewModel>(Empty, Empty));
            _manifest.ViewModelsToViews.Add(CreateMapping<DogViewModel, DogView>(Empty, Empty));
            _manifest.ViewsToPrefabs.Add(CreateMapping<DogView>(DogPrefabUri, Empty));
            
            var variantProvider = Substitute.For<IVariantProvider>();
            variantProvider.AllVariantGroups.Returns(new[] {Temper.Group, Speed.Group}.ToList());
            variantProvider.GlobalVariants.Returns(new ReactiveProperty<VariantSet>(Empty));

            _fixture = new ViewResolver(_manifest, variantProvider, new ScoreEvaluator());
        }

        [Test]
        public void RequestingFastDog_ResolveToFastAnimalView()
        {
            var dog = new Dog();
            var info = _fixture.Resolve(dog, new Options { Variants = Fast });

            Assert.That(info.Model, Is.SameAs(dog));
            Assert.That(info.ModelType, Is.EqualTo(typeof(Dog)));
            Assert.That(info.ViewModel, Is.Null);
            Assert.That(info.ViewModelType, Is.EqualTo(typeof(AnimalViewModel)));
            Assert.That(info.View, Is.Null);
            Assert.That(info.ViewType, Is.EqualTo(typeof(AnimalView)));
            Assert.That(info.PrefabUri, Is.EqualTo(AnimalPrefabUri));
        }

        [Test]
        public void RequestingDog_ResolveToDogView()
        {
            var dog = new Dog();
            var info = _fixture.Resolve(dog, new Options { Variants = Empty });

            Assert.That(info.Model, Is.SameAs(dog));
            Assert.That(info.ModelType, Is.EqualTo(typeof(Dog)));
            Assert.That(info.ViewModel, Is.Null);
            Assert.That(info.ViewModelType, Is.EqualTo(typeof(DogViewModel)));
            Assert.That(info.View, Is.Null);
            Assert.That(info.ViewType, Is.EqualTo(typeof(DogView)));
            Assert.That(info.PrefabUri, Is.EqualTo(DogPrefabUri));
        }

        private TypeToTypeMapping CreateMapping<T, U>(VariantSet explicitVariants = null, VariantSet implicitVariants = null) =>
            new TypeToTypeMapping(typeof(T), typeof(U), explicitVariants ?? Empty) {ImplicitVariants = implicitVariants ?? Empty};

        private TypeToUriMapping CreateMapping<T>(Uri prefabUri, VariantSet explicitVariants = null) =>
            new TypeToUriMapping(typeof(T), prefabUri, explicitVariants ?? Empty);
    }
}