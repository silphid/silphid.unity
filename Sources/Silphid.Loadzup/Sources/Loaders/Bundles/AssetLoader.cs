using System;
using Silphid.Extensions;
using UniRx;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Silphid.Loadzup.Bundles
{
    public class AssetLoader : ILoader
    {
        private const string _pathSeparator = "/";
        private readonly ILoader _innerLoader;

        public bool Supports<T>(Uri uri) =>
            uri.Scheme == Scheme.Bundle && typeof(T) != typeof(AssetBundle);

        public AssetLoader(ILoader innerLoader)
        {
            _innerLoader = innerLoader;
        }

        public IObservable<T> Load<T>(Uri uri, Options options = null)
        {
            var assetName = uri.AbsolutePath.RemovePrefix(_pathSeparator);

            // Todo remove absolutePath in bundle uri
            return _innerLoader.Load<IBundle>(uri, options)
                .ContinueWith(bundle =>
                    typeof(T) == typeof(Scene)
                        ? _innerLoader
                            .Load<Scene>(new Uri($"scene://{assetName}"), options)
                            .Finally(() => (bundle as IDisposable)?.Dispose())
                            .Cast<Scene, T>()
                        : bundle
                            .LoadAsset<T>(assetName)
                            .Finally(() => (bundle as IDisposable)?.Dispose()));
        }
    }
}