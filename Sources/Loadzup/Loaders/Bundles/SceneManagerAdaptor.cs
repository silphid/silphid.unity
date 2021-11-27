using System;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Silphid.Loadzup.Bundles
{
    public class SceneManagerAdaptor : ISceneManager
    {
        public IObservable<AsyncOperation> LoadSceneAsync(int sceneBuildIndex, LoadSceneMode mode) =>
            SceneManager.LoadSceneAsync(sceneBuildIndex, mode)
                        .AsAsyncOperationObservable();

        public IObservable<AsyncOperation> LoadSceneAsync(string sceneName, LoadSceneMode mode) =>
            SceneManager.LoadSceneAsync(sceneName, mode)
                        .AsAsyncOperationObservable();

        public IScene GetSceneByName(string sceneName) =>
            new SceneAdaptor(SceneManager.GetSceneByName(sceneName));

        public IScene GetSceneAt(int sceneBuildIndex) =>
            new SceneAdaptor(SceneManager.GetSceneAt(sceneBuildIndex));
    }
}