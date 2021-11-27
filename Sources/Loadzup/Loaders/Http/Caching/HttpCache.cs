using System;
using System.IO;
using System.Text.RegularExpressions;
using log4net;
using Silphid.Extensions;
using UnityEngine;

namespace Silphid.Loadzup.Http.Caching
{
    public class HttpCache : IHttpCache
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(HttpCache));
        private static string _cacheDir;
        private readonly TimeSpan _defaultTimeToLive;
        private readonly ExpiryDates _expiryDates;

        public HttpCache(TimeSpan defaultTimeToLive)
        {
            _defaultTimeToLive = defaultTimeToLive;
            _expiryDates = ExpiryDates.Load(Path.Combine(GetCacheDir(), "__ExpiryDates.txt"));
        }

        void IHttpCache.Clear()
        {
            Clear();
        }

        public void Invalidate(params CacheGroup[] cacheGroups)
        {
            foreach (var cacheGroup in cacheGroups)
            {
                Log.Info($"Invalidating CacheGroup.{cacheGroup.Name}");
                _expiryDates.Invalidate(cacheGroup.Name);
            }

            _expiryDates.Save();
        }

        public static void Clear()
        {
            Directory.GetFiles(GetCacheDir())
                     .ForEach(File.Delete);

#if UNITY_EDITOR
            Debug.Log("Cache cleared.");
#else
            Log.Info("Cache cleared.");
#endif
        }

        public HttpCacheEntry GetEntry(Uri uri) =>
            HttpCacheEntry.From(uri);

        public bool IsValid(HttpCacheEntry entry) =>
            entry.IsValid(_defaultTimeToLive, _expiryDates);

        public IObservable<byte[]> Load(Uri uri) =>
            HttpCacheEntry.From(uri)
                          .Load();

        public IObservable<Texture2D> LoadTexture(Uri uri) =>
            HttpCacheEntry.From(uri)
                          .LoadTexture();

        public void Save(Uri uri, byte[] bytes, Headers headers)
        {
            if (headers.ContentType == null)
                Log.Warn($"Missing Content-Type in headers of {uri}");

            var entry = HttpCacheEntry.Create(uri, bytes, headers);
            entry.Save();
        }

        internal static string GetContentPath(Uri uri) =>
            GetCacheDir() + Path.DirectorySeparatorChar + GetEscapedFileName(uri);

        internal static string GetCacheDir()
        {
            if (_cacheDir != null)
                return _cacheDir;

            _cacheDir = Application.temporaryCachePath + Path.DirectorySeparatorChar + Application.version +
                        Path.DirectorySeparatorChar + "Silphid.Loadzup.Cache";

            Log.Debug($"Cache path: {_cacheDir}");
            Directory.CreateDirectory(_cacheDir);
            return _cacheDir;
        }

        private static string GetEscapedFileName(Uri uri)
        {
            var invalidCharacters = Regex.Escape(new string(Path.GetInvalidFileNameChars()) + ":");
            var regex = new Regex($"[{invalidCharacters}]");
            return regex.Replace(uri.AbsoluteUri, "_");
        }
    }
}