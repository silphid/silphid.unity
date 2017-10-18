using System;
using System.Collections.Generic;
using log4net;
using Silphid.Extensions;
using UniRx;

namespace Silphid.Loadzup.Caching
{
    public class MemoryCacheLoader : ILoader, IMemoryCache
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(MemoryCacheLoader));
        
        private readonly Dictionary<Uri, Subject<object>> _loadingSubjects = new Dictionary<Uri, Subject<object>>();
        private readonly ILoader _innerLoader;
        private readonly Dictionary<Uri, object> _cache = new Dictionary<Uri, object>();
        private readonly MemoryCachePolicy _defaultPolicy;

        public MemoryCacheLoader(ILoader innerLoader, MemoryCachePolicy? defaultPolicy = null)
        {
            _innerLoader = innerLoader;
            _defaultPolicy = defaultPolicy ?? MemoryCachePolicy.OriginOnly;
        }

        public bool Supports<T>(Uri uri) =>
            _innerLoader.Supports<T>(uri);

        public IObservable<T> Load<T>(Uri uri, Options options = null)
        {
            // Is caching requested in options?
            var policy = options?.MemoryCachePolicy ?? _defaultPolicy;
            if (policy == MemoryCachePolicy.OriginOnly)
                return _innerLoader.Load<T>(uri, options);
            
            lock (this)
            {
                var obj = _cache.GetValueOrDefault(uri);
                if (obj != null)
                {
                    Log.Debug($"{policy} - Loaded from cache - {uri}");
                    return Observable.Return((T) obj);
                }
                
                var subject = _loadingSubjects.GetValueOrDefault(uri);
                if (subject != null)
                {
                    Log.Debug($"{policy} - Already loading from origin - {uri}");
                    return subject.OfType<object, T>();
                }

                _loadingSubjects[uri] = new Subject<object>();
            }

            return _innerLoader
                .Load<T>(uri, options)
                .Do(x => OnLoaded(uri, policy, x))
                .DoOnError(x => OnError(uri, x));
        }

        private void OnLoaded<T>(Uri uri, MemoryCachePolicy policy, T obj)
        {
            Subject<object> subject;

            lock (this)
            {
                Log.Debug($"{policy} - Loaded from origin to cache - {uri}");
                subject = _loadingSubjects[uri];
                _cache[uri] = obj;
                _loadingSubjects.Remove(uri);
            }

            subject.OnNext(obj);
            subject.OnCompleted();
        }

        private void OnError(Uri uri, Exception ex)
        {
            Subject<object> subject;

            lock (this)
            {
                subject = _loadingSubjects[uri];
                _loadingSubjects.Remove(uri);
            }

            subject.OnError(ex);
        }

        public void Clear()
        {
            lock (this)
            {
                _cache.Clear();
            }
        }

        protected void Remove(Uri uri)
        {
            lock (this)
            {
                _cache.Remove(uri);
            }
        }
    }
}