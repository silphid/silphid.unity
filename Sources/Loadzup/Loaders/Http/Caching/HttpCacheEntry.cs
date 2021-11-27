using System;
using System.IO;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;

namespace Silphid.Loadzup.Http.Caching
{
    public class HttpCacheEntry
    {
        private const string HeadersExtension = ".Headers";

        public Uri Uri { get; }
        public byte[] Bytes { get; }
        public string ContentPath { get; }
        public string HeadersPath => ContentPath + HeadersExtension;

        private Headers _headers;

        public Headers Headers
        {
            get { return _headers ?? (_headers = Headers.Load(HeadersPath)); }
            set { _headers = value; }
        }

        private HttpCacheEntry(Uri uri, byte[] bytes = null, Headers headers = null)
            : this(uri, HttpCache.GetContentPath(uri), bytes, headers) {}

        private HttpCacheEntry(Uri uri, string contentPath, byte[] bytes = null, Headers headers = null)
        {
            Uri = uri;
            ContentPath = contentPath;
            Bytes = bytes;
            Headers = headers;
        }

        public static HttpCacheEntry From(Uri uri)
        {
            var path = HttpCache.GetContentPath(uri);
            return File.Exists(path)
                       ? new HttpCacheEntry(uri, path)
                       : null;
        }

        public static HttpCacheEntry Create(Uri uri, byte[] bytes = null, Headers headers = null) =>
            new HttpCacheEntry(uri, bytes, headers);

        public void Save()
        {
            File.WriteAllBytes(ContentPath, Bytes);
            Headers.Save(HeadersPath);
        }

        public IObservable<byte[]> Load() =>
            Observable.Start(() => File.ReadAllBytes(ContentPath))
                      .ObserveOnMainThread();

        public IObservable<Texture2D> LoadTexture() =>
            ObservableWebRequest.GetTexture(new Uri(ContentPath).AbsoluteUri)
                                .Select(x => ((DownloadHandlerTexture) x.downloadHandler).texture);

        public bool IsValid(TimeSpan defaultTimeToLive, ExpiryDates expiryDates)
        {
            Debug.Assert(Headers != null, "Must load headers before checking if expired");

            var fileDate = File.GetLastWriteTimeUtc(ContentPath);

            // Check if file is valid in regard to its cache group's last invalidation date
            if (!expiryDates.IsValid(Headers.E2CacheGroup, fileDate))
                return false;

            // Check if file is valid in regard to its time-to-live duration
            var timeToLive = Headers.CacheControl?.MaxAge ?? defaultTimeToLive;
            return DateTime.UtcNow < fileDate + timeToLive;
        }

        public void Delete()
        {
            File.Delete(ContentPath);
            File.Delete(HeadersPath);
        }
    }
}