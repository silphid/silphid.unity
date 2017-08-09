using System;
using NSubstitute;
using NUnit.Framework;
using Silphid.Loadzup;
using Silphid.Loadzup.Bundles;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoaderTest
{
    private const string SceneName = "testscene";
    private const int SceneIndex = 1;
    private ISceneManager _sceneManager;
    private SceneLoader _fixture;
    private IScene _scene;

    private Uri SceneNameUri => new Uri($"scene://{SceneName}");
    private Uri SceneIndexUri => new Uri($"scene://#{SceneIndex}");
    private Uri WrongSceneIndexUri => new Uri($"scene://#E{SceneIndex}");

    #region SetUp

    [SetUp]
    public void SetUp()
    {
        _sceneManager = Substitute.For<ISceneManager>();
        _fixture = new SceneLoader(_sceneManager);

        _scene = Substitute.For<IScene>();

        _sceneManager.GetSceneByName(Arg.Any<string>())
            .Returns(_scene);
        _sceneManager.GetSceneAt(Arg.Any<int>())
            .Returns(_scene);
    }

    private void SetUpLoadSceneAsync(IObservable<AsyncOperation> returnedObservable)
    {
        _sceneManager.LoadSceneAsync(Arg.Any<int>(), Arg.Any<LoadSceneMode>())
            .Returns(returnedObservable);

        _sceneManager.LoadSceneAsync(Arg.Any<string>(), Arg.Any<LoadSceneMode>())
            .Returns(returnedObservable);
    }

    private void SetUpIsValid(bool isValid = true)
    {
        _scene.IsValid().Returns(isValid);
    }

    #endregion

    #region CheckMethodCalls

    private void CheckReceivedAnyMethodCalls(bool isSceneIndex, LoadSceneMode mode)
    {
        CheckReceivedLoadAsyncCalls(isSceneIndex, mode);
        CheckReceivedGetSceneCalls(isSceneIndex);
    }

    private void CheckReceivedLoadAsyncCalls(bool isSceneIndex, LoadSceneMode mode)
    {
        // Set calls if by index
        var sceneIndexCall = isSceneIndex ? 1 : 0;
        var sceneIndex = isSceneIndex ? SceneIndex : Arg.Any<int>();
        _sceneManager
            .Received(sceneIndexCall)
            .LoadSceneAsync(sceneIndex, mode);

        // Set calls if by name
        var sceneNameCall = !isSceneIndex ? 1 : 0;
        var sceneName = !isSceneIndex ? SceneName : Arg.Any<string>();
        _sceneManager
            .Received(sceneNameCall)
            .LoadSceneAsync(sceneName, mode);
    }

    private void CheckReceivedGetSceneCalls(bool isSceneIndex)
    {
        // Set calls if by index
        var sceneIndexCall = isSceneIndex ? 1 : 0;
        var sceneIndex = isSceneIndex ? SceneIndex : Arg.Any<int>();
        _sceneManager
            .Received(sceneIndexCall)
            .GetSceneAt(sceneIndex);

        // Set calls if by name
        var sceneNameCall = !isSceneIndex ? 1 : 0;
        var sceneName = !isSceneIndex ? SceneName : Arg.Any<string>();
        _sceneManager
            .Received(sceneNameCall)
            .GetSceneByName(sceneName);
    }

    private void CheckDoNotReceivedAnyMethodNoCalls()
    {
        CheckDoNotReceivedLoadAsyncCalls();
        CheckDoNotReceivedGetSceneCalls();
    }

    private void CheckDoNotReceivedLoadAsyncCalls()
    {
        _sceneManager
            .DidNotReceive()
            .LoadSceneAsync(Arg.Any<int>(), Arg.Any<LoadSceneMode>());
        _sceneManager
            .DidNotReceive()
            .LoadSceneAsync(Arg.Any<string>(), Arg.Any<LoadSceneMode>());
    }

    private void CheckDoNotReceivedGetSceneCalls()
    {
        _sceneManager
            .DidNotReceive()
            .GetSceneAt(Arg.Any<int>());
        _sceneManager
            .DidNotReceive()
            .GetSceneByName(Arg.Any<string>());
    }

    #endregion

    private void TestSceneLoadMode(Uri uri, bool isSceneLoadAdditive)
    {
        SetUpIsValid();
        SetUpLoadSceneAsync(Observable.Return(new AsyncOperation()));
        var options = new Options {IsSceneLoadAdditive = isSceneLoadAdditive};

        var result = _fixture.Load<Scene>(uri, options).Wait();

        Assert.That(_scene.Scene, Is.EqualTo(result));
    }

    private void TestSceneLoadModeWithoutOptions(Uri uri)
    {
        SetUpIsValid();
        SetUpLoadSceneAsync(Observable.Return(new AsyncOperation()));

        var result = _fixture.Load<Scene>(uri).Wait();

        Assert.That(_scene.Scene, Is.EqualTo(result));
    }

    [Test]
    public void LoadSceneByIndex_ReturnScene()
    {
        SetUpIsValid();
        SetUpLoadSceneAsync(Observable.Return(new AsyncOperation()));

        var result = _fixture.Load<Scene>(SceneIndexUri).Wait();

        CheckReceivedAnyMethodCalls(true, Arg.Any<LoadSceneMode>());

        Assert.That(_scene.Scene, Is.EqualTo(result));
    }

    [Test]
    public void LoadSceneByName_ReturnScene()
    {
        SetUpIsValid();
        SetUpLoadSceneAsync(Observable.Return(new AsyncOperation()));

        var result = _fixture.Load<Scene>(SceneNameUri).Wait();

        CheckReceivedAnyMethodCalls(false, LoadSceneMode.Additive);

        Assert.That(_scene.Scene, Is.EqualTo(result));
    }

    [Test]
    public void LoadSceneByWrongIndex_ThrowsInvalidOperationException()
    {
        SetUpLoadSceneAsync(Observable.Return(new AsyncOperation()));

        Assert.Throws<InvalidOperationException>(() =>
            _fixture
                .Load<Scene>(WrongSceneIndexUri)
                .Wait());

        CheckDoNotReceivedAnyMethodNoCalls();
    }

    [Test]
    public void LoadSceneByNameWithAdditiveMode_ReturnScene()
    {
        SetUpIsValid();
        TestSceneLoadMode(SceneNameUri, true);
        CheckReceivedAnyMethodCalls(false, LoadSceneMode.Additive);
    }

    [Test]
    public void LoadSceneByNameWithSingleMode_ReturnScene()
    {
        SetUpIsValid();
        TestSceneLoadMode(SceneNameUri, false);
        CheckReceivedAnyMethodCalls(false, LoadSceneMode.Single);
    }

    [Test]
    public void LoadSceneByIndexWithAdditiveMode_ReturnScene()
    {
        SetUpIsValid();
        TestSceneLoadMode(SceneIndexUri, true);
        CheckReceivedAnyMethodCalls(true, LoadSceneMode.Additive);
    }

    [Test]
    public void LoadSceneByIndexWithSingleMode_ReturnScene()
    {
        SetUpIsValid();
        TestSceneLoadMode(SceneIndexUri, false);
        CheckReceivedAnyMethodCalls(true, LoadSceneMode.Single);
    }

    [Test]
    public void LoadSceneByNameWithoutOptions_ReturnScene()
    {
        SetUpIsValid();
        TestSceneLoadModeWithoutOptions(SceneNameUri);
        CheckReceivedAnyMethodCalls(false, LoadSceneMode.Additive);
    }

    [Test]
    public void LoadSceneByIndexWithoutOptions_ReturnScene()
    {
        SetUpIsValid();
        TestSceneLoadModeWithoutOptions(SceneIndexUri);
        CheckReceivedAnyMethodCalls(true, LoadSceneMode.Additive);
    }

    [Test]
    public void CannotLoadSceneByName_ThrowsException()
    {
        SetUpLoadSceneAsync(Observable.Throw<AsyncOperation>(new Exception()));

        Assert.Throws<Exception>(() => _fixture.Load<Scene>(SceneNameUri).Wait());

        CheckReceivedLoadAsyncCalls(false, LoadSceneMode.Additive);
        CheckDoNotReceivedGetSceneCalls();
    }

    [Test]
    public void CannotLoadSceneByIndex_ThrowsException()
    {
        SetUpLoadSceneAsync(Observable.Throw<AsyncOperation>(new Exception()));

        Assert.Throws<Exception>(() => _fixture.Load<Scene>(SceneIndexUri).Wait());

        CheckReceivedLoadAsyncCalls(true, LoadSceneMode.Additive);
        CheckDoNotReceivedGetSceneCalls();
    }

    [Test]
    public void LoadSceneByInvalidName_ReturnInvalidOperation()
    {
        SetUpIsValid(false);
        SetUpLoadSceneAsync(Observable.Return(new AsyncOperation()));

        Assert.Throws<InvalidOperationException>(() =>_fixture.Load<Scene>(SceneNameUri).Wait());
    }

    [Test]
    public void LoadSceneByInvalidIndex_ReturnInvalidOperation()
    {
        SetUpIsValid(false);
        SetUpLoadSceneAsync(Observable.Return(new AsyncOperation()));

        Assert.Throws<InvalidOperationException>(() => _fixture.Load<Scene>(SceneIndexUri).Wait());
    }
}