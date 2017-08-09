using System;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Silphid.Loadzup.Bundles
{
    internal class SceneManagerAdaptor : ISceneManager
    {
        public IObservable<AsyncOperation> LoadSceneAsync(int sceneBuildIndex, LoadSceneMode mode) =>
            SceneManager.LoadSceneAsync(sceneBuildIndex, mode).AsAsyncOperationObservable();

        public IObservable<AsyncOperation> LoadSceneAsync(string sceneName, LoadSceneMode mode) =>
            SceneManager.LoadSceneAsync(sceneName, mode).AsAsyncOperationObservable();

        public IScene GetSceneByName(string sceneName) =>
            new ISceneAdaptor(SceneManager.GetSceneByName(sceneName));

        public IScene GetSceneAt(int sceneBuildIndex) =>
            new ISceneAdaptor(SceneManager.GetSceneAt(sceneBuildIndex));
    }
}