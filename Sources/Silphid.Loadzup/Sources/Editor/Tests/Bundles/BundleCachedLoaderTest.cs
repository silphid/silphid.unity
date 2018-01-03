using System;
using NSubstitute;
using NUnit.Framework;
using Silphid.Loadzup;
using Silphid.Loadzup.Bundles;
using UniRx;

public class BundleCachedLoaderTest
{
    private class MockBundle
    {
        public string Name { get; }
        public IBundle Bundle { get; }
        public MockBundle[] Dependencies { get; }

        public MockBundle(IBundle bundle, string name, MockBundle[] dependencies = null)
        {
            Bundle = bundle;
            Name = name;
            Dependencies = dependencies ?? new MockBundle[0];
        }
    }

    private const string BaseUri = "http://localhost:8080";
    private const string PlatformName = "MockPlatform";
    private const string BundleName = "DefaultBundle";
    private const string DependencyName = "DependencyBundle";
    private MockBundle _mockBundle;
    private ILoader _innerLoader;
    private BundleCacheLoader _fixture;

    private Uri GetBundleUri(string bundleName) => new Uri($"{BaseUri}/{PlatformName}/{bundleName}");

    [SetUp]
    public void SetUp()
    {
        _innerLoader = Substitute.For<ILoader>();

        var platformProvider = Substitute.For<IPlatformProvider>();
        platformProvider.GetPlatformName()
            .Returns(PlatformName);

        _fixture = new BundleCacheLoader(_innerLoader, platformProvider, BaseUri);

        _mockBundle = new MockBundle(Substitute.For<IBundle>(), BundleName);
    }

    private void SetUpInnerLoaderReturns(IObservable<IBundle> returnedObservable)
    {
        _innerLoader.Load<IBundle>(Arg.Any<Uri>(), Arg.Any<Options>())
            .Returns(returnedObservable);
    }

    private MockBundle CreateBundleWithDependencyOn(string bundleName, params MockBundle[] bundles) =>
        new MockBundle(Substitute.For<IBundle>(), bundleName, bundles);

    private void LoadBundleWithDependencyInstantly(MockBundle mockBundle, Options options = null)
    {
        SetUpInnerLoaderReturns(Observable.Return(mockBundle.Bundle));
        _fixture.Load(mockBundle.Name, options ?? new Options())
            .Do(x => (x as IDisposable)?.Dispose())
            .Wait();

        LoadBundleDependencyInstantly(mockBundle, options);
    }

    private void LoadBundleDependencyInstantly(MockBundle mockBundle, Options options = null)
    {
        foreach (var dependency in mockBundle.Dependencies)
        {
            SetUpInnerLoaderReturns(Observable.Return(dependency.Bundle));
            _fixture.LoadDependency(dependency.Name, options ?? new Options(), mockBundle.Name)
                .Do(x => (x as IDisposable)?.Dispose())
                .Wait();
        }
    }

    private IObservable<IBundle> LoadBundle(MockBundle bundle) => _fixture.Load(bundle.Name, new Options());

    private IObservable<IBundle> LoadBundleDependency(MockBundle bundle)
        => _fixture.LoadDependency(bundle.Name, new Options(), bundle.Name);

    private bool UnloadBundleWithDependency(MockBundle bundle)
    {
        if (!_fixture.Unload(bundle.Name))
            return false;

        UnloadDependency(bundle);
        return true;
    }

    private void UnloadDependency(MockBundle bundle)
    {
        foreach (var dependency in bundle.Dependencies)
            _fixture.UnloadDependency(dependency.Name, bundle.Name);
    }

    [Test]
    public void CheckUriPassedToInnerLoader_ReturnRightUri()
    {
        LoadBundleWithDependencyInstantly(_mockBundle);
        _innerLoader.Received(1).Load<IBundle>(GetBundleUri(_mockBundle.Name), Arg.Any<Options>());
    }

    [Test]
    public void LoadBundle_ReturnBundle()
    {
        LoadBundleWithDependencyInstantly(_mockBundle);
        _innerLoader.Received(1).Load<IBundle>(Arg.Any<Uri>(), Arg.Any<Options>());
    }

    [Test]
    public void UnloadBundleIfNotLoadedYet_ReturnFalse()
    {
        Assert.IsFalse(UnloadBundleWithDependency(_mockBundle));
    }

    [Test]
    public void UnloadBundleIfNoDependency_DoUnloadBundle()
    {
        LoadBundleWithDependencyInstantly(_mockBundle);

        Assert.IsTrue(UnloadBundleWithDependency(_mockBundle));
        _mockBundle.Bundle.Received(1).Unload();
    }

    [Test]
    public void CheckOptionsPassedToInnerLoader_ReturnRightOptions()
    {
        var options = new Options();
        LoadBundleWithDependencyInstantly(_mockBundle, options);

        _innerLoader.Received(1).Load<IBundle>(GetBundleUri(_mockBundle.Name), options);
    }

    [Test]
    public void LoadBundle_LoadDependencyOn_UnloadBundle_DoNotUnloadBundle()
    {
        var bundleWithDependencyOnMock = CreateBundleWithDependencyOn(DependencyName, _mockBundle);

        // Create Bundle
        LoadBundleWithDependencyInstantly(_mockBundle);

        // Create DependencyOn
        LoadBundleWithDependencyInstantly(bundleWithDependencyOnMock);

        // Unload Bundle
        UnloadBundleWithDependency(_mockBundle);
        _mockBundle.Bundle.DidNotReceive().Unload();
    }

    [Test]
    public void LoadBundle_LoadDependencyOn_UnloadBundle_UnloadDependencyOn_DoUnloadBundle()
    {
        var bundleWithDependencyOnMock = CreateBundleWithDependencyOn(DependencyName, _mockBundle);

        LoadBundleWithDependencyInstantly(_mockBundle);
        LoadBundleWithDependencyInstantly(bundleWithDependencyOnMock);

        // Unloading Bundle First
        UnloadBundleWithDependency(_mockBundle);
        _mockBundle.Bundle.DidNotReceive().Unload();

        // Unload DependencyOn
        UnloadBundleWithDependency(bundleWithDependencyOnMock);
        _mockBundle.Bundle.Received(1).Unload();
    }

    [Test]
    public void LoadBundle_LoadDependencyOn_UnloadDependency_UnloadBundle_DoUnloadBundle()
    {
        var bundleWithDependencyOnMock = CreateBundleWithDependencyOn(DependencyName, _mockBundle);

        LoadBundleWithDependencyInstantly(_mockBundle);
        LoadBundleWithDependencyInstantly(bundleWithDependencyOnMock);

        // Unload DependencyOn First
        UnloadBundleWithDependency(bundleWithDependencyOnMock);
        _mockBundle.Bundle.DidNotReceive().Unload();

        UnloadBundleWithDependency(_mockBundle);
        _mockBundle.Bundle.Received(1).Unload();
    }

    [Test]
    public void LoadBundle_LoadDependencyOn_UnloadDependency_LoadDependencyOn_UnloadBundle_DoNotUnloadBundle()
    {
        var firstBundleWithDependencyOnMock = CreateBundleWithDependencyOn(DependencyName, _mockBundle);
        var secondBundleWithDependencyOnMock = CreateBundleWithDependencyOn(DependencyName,_mockBundle);

        LoadBundleWithDependencyInstantly(_mockBundle);
        LoadBundleWithDependencyInstantly(firstBundleWithDependencyOnMock);

        // Unloading Dependency First
        UnloadBundleWithDependency(firstBundleWithDependencyOnMock);
        _mockBundle.Bundle.DidNotReceive().Unload();

        LoadBundleWithDependencyInstantly(secondBundleWithDependencyOnMock);

        // Unload Bundle
        UnloadBundleWithDependency(_mockBundle);

        _mockBundle.Bundle.Received(0).Unload();
    }

    [Test]
    public void LoadDependencyOn_LoadBundle_UnloadDependency_UnloadBundle_DoUnloadBundle()
    {
        var bundleWithDependencyOnMock = CreateBundleWithDependencyOn(DependencyName, _mockBundle);

        LoadBundleWithDependencyInstantly(bundleWithDependencyOnMock);
        LoadBundleWithDependencyInstantly(_mockBundle);

        // Unloading Dependency First
        UnloadBundleWithDependency(bundleWithDependencyOnMock);
        _mockBundle.Bundle.DidNotReceive().Unload();

        // Unload Bundle
        UnloadBundleWithDependency(_mockBundle);
        _mockBundle.Bundle.Received(1).Unload();
    }

    [Test]
    public void LoadDependencyOn_UnloadDependency_DoUnloadBundle()
    {
        var bundleWithDependencyOnMock = CreateBundleWithDependencyOn(DependencyName, _mockBundle);

        LoadBundleWithDependencyInstantly(bundleWithDependencyOnMock);

        UnloadBundleWithDependency(bundleWithDependencyOnMock);

        _mockBundle.Bundle.Received(1).Unload();
    }

    [Test]
    public void LoadBundleAndWait_UnloadBundle_CompleteBundleLoading_DoNotUnloadUntilLoadingDone()
    {
        var loadingSubject = new Subject<IBundle>();
        SetUpInnerLoaderReturns(loadingSubject);

        IDisposable loadingDisposable = null;
        LoadBundle(_mockBundle)
            .Subscribe(x => loadingDisposable = x as IDisposable);

        // Unload Bundle
        UnloadBundleWithDependency(_mockBundle);

        // Finish loading return bundle
        loadingSubject.OnNext(_mockBundle.Bundle);
        loadingSubject.OnCompleted();

        // Do not unload bundle
        _mockBundle.Bundle.Received(0).Unload();

        // Dispose loading
        loadingDisposable.Dispose();

        // Unload bundle
        _mockBundle.Bundle.Received(1).Unload();
    }

    [Test]
    public void
        LoadBundleAndWait_LoadDependencyOn_UnloadBundle_CompleteBundleLoading_DoNotUnloadUntilDependencyUnloadingDone()
    {
        var bundleWithDependencyOnMock = CreateBundleWithDependencyOn(DependencyName, _mockBundle);

        SetUpInnerLoaderReturns(Observable.Return(_mockBundle.Bundle));

        var loadingDisposable = (IDisposable) LoadBundle(_mockBundle).Wait();

        // Load Bundle with dependencyOn instantly
        LoadBundleWithDependencyInstantly(bundleWithDependencyOnMock);

        // Unload Bundle
        UnloadBundleWithDependency(_mockBundle);

        // Do not unload bundle
        _mockBundle.Bundle.Received(0).Unload();

        // Dispose loading
        loadingDisposable.Dispose();

        // Do not unload bundle because there is a dependency;
        _mockBundle.Bundle.Received(0).Unload();

        // Unload Dependency
        UnloadBundleWithDependency(bundleWithDependencyOnMock);

        _mockBundle.Bundle.Received(1).Unload();
    }

    [Test]
    public void DoNotUnloadDependencyTwiceOfSameBundle_DoNotUnloadDependencyBundle()
    {
        // B has dependency to A
        // C has dependency to A
        // D has dependency to B so it has to A
        // Unload B and his dependency TWICE
        // A should not be unload because it is a dependency of C
        // B should not be unload because it a dependency of D

        var bundleA = new MockBundle(Substitute.For<IBundle>(), "A");

        var bundleB = CreateBundleWithDependencyOn("B", bundleA);
        LoadBundleWithDependencyInstantly(bundleB);

        // Create dependency bundle to keep A alive
        var bundleC = CreateBundleWithDependencyOn("C", bundleA);
        LoadBundleWithDependencyInstantly(bundleC);

        // Create dependency bundle to keep B alive (if not releaseRef will return false because B won't be loaded)
        var bundleD = CreateBundleWithDependencyOn("D", bundleB);
        LoadBundleWithDependencyInstantly(bundleD);

        UnloadBundleWithDependency(bundleB);
        bundleB.Bundle.Received(0).Unload(); // D is dependent on it
        bundleA.Bundle.Received(0).Unload(); // C is dependent on it

        // Unload bundle twice (should not decrease dependency ref and keep it loaded)
        UnloadBundleWithDependency(bundleB);
        bundleB.Bundle.Received(0).Unload(); // D is dependent on it
        bundleA.Bundle.Received(0).Unload(); // C is dependent on it
    }

    [Test]
    public void UnloadBundleOnlyIfNoMoreDependency_UnloadBundle()
    {
        // B has dependency to A
        // C has dependency to A
        // D has dependency to B so it has to A
        // Unload B and his dependency TWICE
        // A should not be unload because it is a dependency of C
        // B should not be unload because it a dependency of D

        // Will get load only from dependency
        var bundleA = new MockBundle(Substitute.For<IBundle>(), "A");

        var bundleB = CreateBundleWithDependencyOn("B", bundleA);
        LoadBundleWithDependencyInstantly(bundleB);

        // Create dependency bundle to keep A alive
        var bundleC = CreateBundleWithDependencyOn("C", bundleA);
        LoadBundleWithDependencyInstantly(bundleC);

        // Create dependency bundle to keep B alive (if not releaseRef will return false because B won't be loaded)
        // bundleD is also dependent on A because of bundleB
        var bundleD = CreateBundleWithDependencyOn("D", bundleB, bundleA);
        LoadBundleWithDependencyInstantly(bundleD);

        UnloadBundleWithDependency(bundleB);
        bundleB.Bundle.Received(0).Unload(); // D is dependent on B
        bundleA.Bundle.Received(0).Unload(); // B is dependent on A

        UnloadBundleWithDependency(bundleC);
        bundleC.Bundle.Received(1).Unload(); // No more dependency
        bundleA.Bundle.Received(0).Unload(); // D is dependent on B that is dependent on A

        UnloadBundleWithDependency(bundleD);
        bundleD.Bundle.Received(1).Unload(); // No more dependency
        bundleB.Bundle.Received(1).Unload(); // No more dependency
        bundleA.Bundle.Received(1).Unload(); // No more dependency
    }
}