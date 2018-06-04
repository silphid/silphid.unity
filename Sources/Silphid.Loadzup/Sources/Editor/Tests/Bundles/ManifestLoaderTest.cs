using System;
using NSubstitute;
using NUnit.Framework;
using Silphid.Loadzup;
using Silphid.Loadzup.Bundles;
using UniRx;
using UnityEngine;

public class ManifestLoaderTest
{
    private const string BaseUri = "http://localhost:8080";
    private ILoader _innerLoader;
    private BundleManifestLoader _fixture;
    private IPlatformProvider _platformProvider;
    private IBundle _bundle;

    [SetUp]
    public void SetUp()
    {
        _innerLoader = Substitute.For<ILoader>();
        _platformProvider = Substitute.For<IPlatformProvider>();
        _bundle = Substitute.For<IBundle>();
    }

    private void SetupLoaderReturn(IObservable<IBundle> returnObservable)
    {
        _innerLoader.Load<IBundle>(Arg.Any<Uri>())
            .Returns(returnObservable);
    }

    private void SetupBundleReturn(IObservable<AssetBundleManifest> returnObservable)
    {
        _bundle.LoadAsset<AssetBundleManifest>(Arg.Any<string>())
            .Returns(returnObservable);
    }

    private void SetupPlatformProvider(string platformName)
    {
        _platformProvider.GetPlatformName()
            .Returns(platformName);
    }

    private void CreateFixture()
    {
        _fixture = new BundleManifestLoader(_innerLoader, _platformProvider, BaseUri);
    }

    [Test]
    public void ErrorLoadingBundle_ReturnInvalidOperationException()
    {
        CreateFixture();
        SetupLoaderReturn(Observable.Throw<IBundle>(new InvalidOperationException()));
//        SetupBundleReturn(Observable.Return(new AssetBundleManifest()));

        Assert.Throws<InvalidOperationException>(() => _fixture.Load().Wait());

        _innerLoader.Received(1).Load<IBundle>(Arg.Any<Uri>());
        _bundle.DidNotReceive().LoadAsset<AssetBundleManifest>(Arg.Any<string>());
    }

    [Test]
    public void AssetBundleManifestIsNull_ReturnInvalidOperationException()
    {
        CreateFixture();
        SetupLoaderReturn(Observable.Return(_bundle));
        SetupBundleReturn(Observable.Return((AssetBundleManifest) null));

        Assert.Throws<InvalidOperationException>(() => _fixture.Load().Wait());

        _innerLoader.Received(1).Load<IBundle>(Arg.Any<Uri>());
        _bundle.Received(1).LoadAsset<AssetBundleManifest>(Arg.Any<string>());
    }

    [Test]
    public void CheckUriPassedToLoader_ReturnValidUri()
    {
        const string platformName = "Windows";
        var uri = new Uri($"{BaseUri}/{platformName}/{platformName}");
        SetupPlatformProvider(platformName);

        CreateFixture();

        // SetupReturn to exception because we can't test IBundle.LoadAsset with a new AssetBundleManifest (Unknown Unity issue?)
        SetupLoaderReturn(Observable.Throw<IBundle>(new InvalidOperationException()));
        SetupBundleReturn(Observable.Return((AssetBundleManifest)null));

        Assert.Throws<InvalidOperationException>(() => _fixture.Load().Wait());

        _innerLoader.Received(1).Load<IBundle>(uri);
    }
}