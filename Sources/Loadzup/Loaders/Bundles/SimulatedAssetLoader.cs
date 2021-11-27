using System;
using UniRx;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using Silphid.Extensions;

#endif

namespace Silphid.Loadzup.Bundles
{
    public class SimulatedAssetLoader : ILoader
    {
        private const string _pathSeparator = "/";

        public bool Supports<T>(Uri uri) =>
            uri.Scheme == Scheme.Bundle && typeof(T) != typeof(AssetBundle);

        public IObservable<T> Load<T>(Uri uri, IOptions options = null)
        {
#if UNITY_EDITOR
            var assetBundleName = uri.Host;
            var assetName = uri.AbsolutePath.RemovePrefix(_pathSeparator);

            var assetPaths = AssetDatabase.GetAssetPathsFromAssetBundleAndAssetName(assetBundleName, assetName);
            if (assetPaths.Length == 0)
            {
                throw new ArgumentNullException(
                    $"#SimulatedAssetLoader# There is no asset with name \"{assetName}\" in {assetBundleName}");
            }

            return Observable.Return((T) (object) AssetDatabase.LoadMainAssetAtPath(assetPaths[0]));
#else
            return Observable.Return((T)(object)null);
#endif
        }
    }
}