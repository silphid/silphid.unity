using System;
using Silphid.Extensions;
using UniRx;
using UnityEngine.SceneManagement;

namespace Silphid.Loadzup.Bundles
{
    public class AssetLoader : ILoader
    {
        private const string _pathSeparator = "/";
        private readonly ILoader _innerLoader;

        private string GetAssetName(Uri uri) => uri.AbsolutePath.RemovePrefix(_pathSeparator);

        public bool Supports<T>(Uri uri) =>
            uri.Scheme == Scheme.Bundle && !string.IsNullOrWhiteSpace(GetAssetName(uri));

        public AssetLoader(ILoader innerLoader)
        {
            _innerLoader = innerLoader;
        }

        public IObservable<T> Load<T>(Uri uri, IOptions options = null)
        {
            if (!Supports<T>(uri))
                throw new NotSupportedException($"Uri not supported: {uri}");

            var assetName = GetAssetName(uri);

            // Todo remove absolutePath in bundle uri
            return _innerLoader.Load<IBundle>(uri, options)
                               .ContinueWith(
                                    bundle => typeof(T) == typeof(Scene)
                                                  ? _innerLoader.Load<Scene>(new Uri($"scene://{assetName}"), options)
                                                                .Cast<Scene, T>()
                                                  : bundle.LoadAsset<T>(assetName));
        }
    }
}