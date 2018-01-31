using System;
using NSubstitute;
using NUnit.Framework;
using Silphid.Loadzup.Bundles;
using UniRx;
using UnityEngine.SceneManagement;

namespace Silphid.Loadzup.Test.Bundles
{
    public class AssetLoaderTest
    {
        private readonly string _assetName = "test-assetbundle";
        private readonly IBundle _bundle = Substitute.For<IBundle>();
        private readonly Scene _scene = new Scene();
        private ILoader _innerLoader;
        private AssetLoader _fixture;

        private IObservable<Scene> GetSceneObservable() => Observable.Return(_scene);
        private Uri GetBundleUri(string assetName) => new Uri($"bundle://test-bundle/{assetName}");
        private Uri GetSceneUri(string sceneName) => new Uri($"scene://{sceneName}");

        [SetUp]
        public void SetUp()
        {
            _innerLoader = Substitute.For<ILoader>();
            _fixture = new AssetLoader(_innerLoader);

            _innerLoader.Load<IBundle>(Arg.Any<Uri>(), Arg.Any<Options>())
                .Returns(Observable.Return(_bundle));
        }

        private void SetUpSceneLoader(IObservable<Scene> observable)
        {
            _innerLoader.Load<Scene>(GetSceneUri(_assetName), Arg.Any<Options>())
                .Returns(observable);
        }

        private void SetUpBundleLoadAssetReturn<T>(T asset)
        {
            _bundle.LoadAsset<T>(Arg.Any<string>())
                .Returns(Observable.Return(asset));
        }

        [Test]
        public void LoadSceneIfNotAsset_ReturnScene()
        {
            SetUpSceneLoader(GetSceneObservable());

            var scene = _fixture.Load<Scene>(GetBundleUri(_assetName)).Wait(TimeSpan.FromSeconds(5));

            Assert.AreEqual(_scene, scene);
        }

        // [Ignore]
        [Test]
        public void LoadAssetIfNotScene_ReturnAsset()
        {
            throw new NotImplementedException();
        }

        // [Ignore]
        [Test]
        public void CheckUriAndOptionsPassedToLoaderWhenScene_ReturnValidArgs()
        {
            throw new NotImplementedException();
        }

        [Test]
        public void CheckAssetNamePassedToBundle_ReturnValidAssetName()
        {
            SetUpBundleLoadAssetReturn("test");
            _fixture.Load<string>(GetBundleUri(_assetName)).Wait(TimeSpan.FromSeconds(5));

            _bundle.Received(1).LoadAsset<string>(_assetName);
        }

        [Test]
        public void CheckAssetNamePassedToSceneLoader_ReturnValidAssetName()
        {
            SetUpSceneLoader(GetSceneObservable());

            _fixture.Load<Scene>(GetBundleUri(_assetName)).Wait(TimeSpan.FromSeconds(5));

            _innerLoader.Received(1).Load<Scene>(GetSceneUri(_assetName), Arg.Any<Options>());
        }

        //[Ignore]
        [Test]
        public void ErrorLoadingBundle_DoNotCallBundleNorSceneLoader()
        {
            throw new NotImplementedException();
        }
    }
}