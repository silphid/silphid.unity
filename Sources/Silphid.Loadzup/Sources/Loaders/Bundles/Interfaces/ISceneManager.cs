using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Silphid.Loadzup.Bundles
{
    public interface ISceneManager
    {
        IObservable<AsyncOperation> LoadSceneAsync(int sceneBuildIndex, LoadSceneMode mode);
        IObservable<AsyncOperation> LoadSceneAsync(string sceneName, LoadSceneMode mode);
        IScene GetSceneByName(string sceneName);
        IScene GetSceneAt(int sceneBuildIndex);
    }
}