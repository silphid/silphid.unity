using System;
using Silphid.Extensions;
using UniRx;
using UnityEngine.SceneManagement;

namespace Silphid.Loadzup.Bundles
{
    public class SceneLoader : ILoader
    {
        private const string SceneBuildIndexPrefix = "#";
        private readonly ISceneManager _sceneManager;

        public bool Supports<T>(Uri uri) =>
            uri.Scheme == Scheme.Scene;

        public SceneLoader(ISceneManager sceneManager)
        {
            _sceneManager = sceneManager;
        }

        public IObservable<T> Load<T>(Uri uri, Options options = null) =>
            LoadInternal(uri, options?.IsAdditiveSceneLoading == false ? LoadSceneMode.Single : LoadSceneMode.Additive)
                .Cast<Scene, T>();

        private IObservable<Scene> LoadInternal(Uri uri, LoadSceneMode mode) =>
            string.IsNullOrEmpty(uri.Fragment)
                ? LoadSceneByName(uri.Host, mode)
                : LoadSceneByIndex(GetSceneBuildIndex(uri), mode);

        private IObservable<Scene> LoadSceneByName(string sceneName, LoadSceneMode mode) =>
            _sceneManager
                .LoadSceneAsync(sceneName, mode)
                .Select(_ =>
                {
                    var scene = _sceneManager.GetSceneByName(sceneName);

                    if (!scene.IsValid())
                        throw new InvalidOperationException(
                            $"#SceneLoader# The scene named \"{sceneName}\" is invalid");

                    return scene.Scene;
                });

        private int GetSceneBuildIndex(Uri uri)
        {
            int sceneBuildIndex;
            if (!int.TryParse(uri.Fragment.RemovePrefix(SceneBuildIndexPrefix), out sceneBuildIndex))
                throw new InvalidOperationException($"Failed to parse scene build index from: {uri}");

            return sceneBuildIndex;
        }

        private IObservable<Scene> LoadSceneByIndex(int sceneBuildIndex, LoadSceneMode mode) =>
            _sceneManager
                .LoadSceneAsync(sceneBuildIndex, mode)
                .Select(_ =>
                {
                    var scene = _sceneManager.GetSceneAt(sceneBuildIndex);

                    if (!scene.IsValid())
                        throw new InvalidOperationException($"Invalid scene at index {sceneBuildIndex}");

                    return scene.Scene;
                });
    }
}