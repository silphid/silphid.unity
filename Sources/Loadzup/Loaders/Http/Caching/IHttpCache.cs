using System;
using UnityEngine;

namespace Silphid.Loadzup.Http.Caching
{
    public interface IHttpCache
    {
        void Clear();
        void Invalidate(params CacheGroup[] cacheGroups);
        HttpCacheEntry GetEntry(Uri uri);
        bool IsValid(HttpCacheEntry entry);
        IObservable<byte[]> Load(Uri uri);
        IObservable<Texture2D> LoadTexture(Uri uri);
        void Save(Uri uri, byte[] bytes, Headers headers);
    }
}