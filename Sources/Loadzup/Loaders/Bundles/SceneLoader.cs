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

        public IObservable<T> Load<T>(Uri uri, IOptions options = null) =>
            LoadInternal(uri, options.GetLoadSceneMode())
               .Cast<Scene, T>();

        private IObservable<Scene> LoadInternal(Uri uri, LoadSceneMode mode)
        {
            // Check if sceneName is a sceneBuildIndex
            if (!string.IsNullOrEmpty(uri.Fragment))
            {
                int sceneBuildIndex;
                if (int.TryParse(uri.Fragment.RemovePrefix(SceneBuildIndexPrefix), out sceneBuildIndex))
                    return _sceneManager.LoadSceneAsync(sceneBuildIndex, mode)
                                        .Select(
                                             _ =>
                                             {
                                                 var scene = _sceneManager.GetSceneAt(sceneBuildIndex);

                                                 if (!scene.IsValid())
                                                     throw new InvalidOperationException(
                                                         $"#SceneLoader# The scene with index \"{sceneBuildIndex}\" is invalid");

                                                 return scene.Scene;
                                             });

                throw new InvalidOperationException("#SceneLoader# Cannot parse sceneBuildIndex");
            }

            var sceneName = uri.Host;
            return _sceneManager.LoadSceneAsync(sceneName, mode)
                                .Select(
                                     _ =>
                                     {
                                         var scene = _sceneManager.GetSceneByName(sceneName);

                                         if (!scene.IsValid())
                                             throw new InvalidOperationException(
                                                 $"#SceneLoader# The scene named \"{sceneName}\" is invalid");

                                         return scene.Scene;
                                     });
        }
    }
}