using System;
using NSubstitute;
using NUnit.Framework;
using Silphid.Loadzup;
using Silphid.Loadzup.Bundles;
using UniRx;

public class BundleLoaderTest
{
    private const string AssetBundleName = "test-bundle";
    private readonly Options _mockedOptions = new Options();
    private readonly string[] _mockedDependencies = {"a", "b", "c", "d"};
    private readonly IBundle _mockBundle = Substitute.For<IBundle>();
    private IBundleCachedLoader _cachedLoader;
    private BundleLoader _fixture;
    private IBundleManifest _bundleManifest;
    private IBundleManifestLoader _bundleManifestLoader;

    private Uri GetBundleUri() => new Uri($"bundle://{AssetBundleName}");
    private IObservable<IBundle> GetBundleObservable() => Observable.Return(_mockBundle);

    [SetUp]
    public void SetUp()
    {
        _cachedLoader = Substitute.For<IBundleCachedLoader>();

        // Mock bundleManifest
        _bundleManifestLoader = Substitute.For<IBundleManifestLoader>();
        _bundleManifest = Substitute.For<IBundleManifest>();
        _bundleManifestLoader.Load().Returns(Observable.Return(_bundleManifest));

        _fixture = new BundleLoader(_cachedLoader, _bundleManifestLoader);
    }

    private void SetupLoadReturns(IObservable<IBundle> observable)
    {
        _cachedLoader.Load(Arg.Any<string>(), Arg.Any<Options>())
            .Returns(observable);
    }

    private void SetupLoadDependencyReturns(IObservable<IBundle> observable, string bundleName = null)
    {
        _cachedLoader.LoadDependency(bundleName ?? Arg.Any<string>(), Arg.Any<Options>(), Arg.Any<string>())
            .Returns(observable);
    }

    private void SetupManifestDependencyBundlesReturn(string[] dependentBundles = null, string bundleName = null)
    {
        _bundleManifest.GetAllDependencies(bundleName ?? Arg.Any<string>())
            .Returns(dependentBundles ?? new string[0]);
    }

    private void SetupLoadAndLoadDependencyReturns()
    {
        SetupLoadReturns(GetBundleObservable());
        SetupLoadDependencyReturns(GetBundleObservable());
    }

    [Test]
    public void CheckArgsPassedToLoader_CallLoadWithValidArgs()
    {
        SetupLoadAndLoadDependencyReturns();

        // No dependencies
        SetupManifestDependencyBundlesReturn();

        _fixture.Load<IBundle>(GetBundleUri(), _mockedOptions).Wait();
        _cachedLoader.Received(1).Load(AssetBundleName, _mockedOptions);
    }

    [Test]
    public void CheckArgsPassedWithoutOptionsToLoader_CallLoadWithValidArgs()
    {
        SetupLoadAndLoadDependencyReturns();

        // No dependencies
        SetupManifestDependencyBundlesReturn();

        _fixture.Load<IBundle>(GetBundleUri()).Wait();
        _cachedLoader.Received(1).Load(AssetBundleName, Arg.Any<Options>());
    }

    [Test]
    public void LoadAllDependencies_ReturnLoadAllDependenciesWithExactArgs()
    {
        SetupLoadAndLoadDependencyReturns();

        // Mock dependencies
        SetupManifestDependencyBundlesReturn(_mockedDependencies);

        _fixture.Load<IBundle>(GetBundleUri(), _mockedOptions).Wait();

        _cachedLoader.Received(1).Load(AssetBundleName, _mockedOptions);

        foreach (var t in _mockedDependencies)
            _cachedLoader.Received(1).LoadDependency(t, _mockedOptions, Arg.Any<string>());
    }

    [Test]
    public void ErrorLoadingBundle_ThrowsException()
    {
        SetupLoadReturns(Observable.Throw<IBundle>(new Exception()));
        SetupLoadDependencyReturns(GetBundleObservable());

        SetupManifestDependencyBundlesReturn();

        Assert.Throws<Exception>(() => _fixture.Load<IBundle>(GetBundleUri(), _mockedOptions).Wait());
    }

    [Test]
    public void ErrorLoadingDependency_ThrowsException()
    {
        SetupLoadReturns(GetBundleObservable());
        SetupLoadDependencyReturns(Observable.Throw<IBundle>(new Exception()));

        // Mock dependencies
        SetupManifestDependencyBundlesReturn(_mockedDependencies);

        Assert.Throws<Exception>(() => _fixture.Load<IBundle>(GetBundleUri(), _mockedOptions).Wait());
    }

    [Test]
    public void DoNotUnloadBundleThatHasNotBeenLoaded_DoNotCallLoadManifest()
    {
        _fixture.Unload(AssetBundleName);

        Assert.IsFalse(_cachedLoader.Received(1).Unload(Arg.Any<string>()));
        _bundleManifestLoader.DidNotReceive().Load();
        _bundleManifest.DidNotReceive().GetAllDependencies(Arg.Any<string>());
        _cachedLoader.DidNotReceive().UnloadDependency(Arg.Any<string>(), Arg.Any<string>());
    }

    [Test]
    public void UnloadBundleAndDependencies_CallLoadManifestAndUnloadDependencies()
    {
        SetupManifestDependencyBundlesReturn(_mockedDependencies);
        _cachedLoader.Unload(Arg.Any<string>()).Returns(true);

        _fixture.Unload(AssetBundleName);
        
        _cachedLoader.Received(1).Unload(Arg.Any<string>());
        _bundleManifestLoader.Received(1).Load();
        _bundleManifest.Received(1).GetAllDependencies(Arg.Any<string>());

        foreach (var t in _mockedDependencies)
            _cachedLoader.Received(1).UnloadDependency(t, Arg.Any<string>());
    }

    [Test]
    public void ErrorLoadingBundle_UnloadAllDependencies()
    {
        SetupLoadReturns(Observable.Throw<IBundle>(new InvalidOperationException()));

        SetupLoadDependencyReturns(GetBundleObservable());
        SetupManifestDependencyBundlesReturn(_mockedDependencies);

        Assert.Throws<InvalidOperationException>(() => _fixture.Load<IBundle>(GetBundleUri()).Wait());

        foreach (var dependency in _mockedDependencies)
        {
            _cachedLoader.Received(1).UnloadDependency(dependency, Arg.Any<string>());
        }
    }

    [Test]
    public void ErrorLoadingDependency_UnloadOnlyLoadedDependencies()
    {
        const int throwErrorOnDependencyIndex = 2;
        SetupManifestDependencyBundlesReturn(_mockedDependencies);

        SetupLoadReturns(GetBundleObservable());

        // SetupLoadDependencyReturns
        // Create subject to throw error when loading 3rd dependency only
        var errorSubject = new Subject<IBundle>();
        for (var i = 0; i < _mockedDependencies.Length; i++)
            SetupLoadDependencyReturns(i == throwErrorOnDependencyIndex ? errorSubject : GetBundleObservable(),
                _mockedDependencies[i]);

        _fixture.Load<IBundle>(GetBundleUri()).Subscribe();

        Assert.Throws<InvalidOperationException>(() => errorSubject.OnError(new InvalidOperationException()));

        // Unload only loaded dependencies
        for (var i = 0; i < _mockedDependencies.Length; i++)
        {
            _cachedLoader.Received(1).LoadDependency(_mockedDependencies[i], Arg.Any<Options>(), Arg.Any<string>());
            _cachedLoader.Received(i == throwErrorOnDependencyIndex ? 0 : 1).UnloadDependency(_mockedDependencies[i], Arg.Any<string>());
        }
    }
}