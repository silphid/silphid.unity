using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Silphid.Loadzup.Caching
{
    public class CachedLoader : ILoader
    {
        protected readonly ILoader _innerLoader;
        protected readonly Dictionary<Uri, object> _cache = new Dictionary<Uri, object>();

        public CachedLoader(ILoader innerLoader)
        {
            _innerLoader = innerLoader;
        }

        public bool Supports(Uri uri) =>
            _innerLoader.Supports(uri);

        public UniRx.IObservable<T> Load<T>(Uri uri, Options options = null) =>
            LoadInternal<T>(uri, options).Select(GetInstance);

        private UniRx.IObservable<T> LoadInternal<T>(Uri uri, Options options)
        {
            object obj;
            if (_cache.TryGetValue(uri, out obj))
                return Observable.Return((T) obj);

            return _innerLoader
                .Load<T>(uri, options)
                .Do(x => _cache[uri] = x);
        }

        private T GetInstance<T>(T obj) =>
            obj is GameObject
                ? (T) (object) Object.Instantiate((GameObject) (object) obj)
                : obj;

        public void ClearCache()
        {
            _cache.Clear();
        }
    }
}