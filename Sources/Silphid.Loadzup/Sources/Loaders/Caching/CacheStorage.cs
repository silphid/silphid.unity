using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using log4net;
using Silphid.Extensions;
using UniRx;
using UnityEngine;

namespace Silphid.Loadzup.Caching
{
    public class CacheStorage : ICacheStorage
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(CacheStorage));

        private const string HeadersExtension = ".Headers";
        private string _cacheDir;

        public IObservable<Unit> DeleteExpired(DateTime utcNow, TimeSpan defaultExpirySpan) =>
            Observable.Start(() => DeleteExpiredInternal(utcNow, defaultExpirySpan), Scheduler.ThreadPool);

        public void DeleteAll()
        {
            Directory
                .GetFiles(GetCacheDir())
                .ForEach(File.Delete);
        }

        public bool Contains(Uri uri) => File.Exists(GetFilePath(uri));

        public byte[] Load(Uri uri) =>
            File.ReadAllBytes(GetFilePath(uri));

        public Dictionary<string, string> LoadHeaders(Uri uri) =>
            LoadHeaders(GetHeadersFile(GetFilePath(uri)));

        private void DeleteExpiredInternal(DateTime utcNow, TimeSpan defaultExpirySpan)
        {
            int deletedCount = 0;
            
            Directory
                .GetFiles(GetCacheDir())
                .Where(file => file.EndsWith(HeadersExtension))
                .ForEach(headersFile =>
                {
                    var mainFile = headersFile.RemoveSuffix(HeadersExtension);
                    var fileDate = File.GetLastWriteTimeUtc(mainFile);
                    var headers = LoadHeaders(headersFile);

                    if (IsExpired(headers, utcNow, fileDate, defaultExpirySpan))
                    {
                        File.Delete(mainFile);
                        File.Delete(headersFile);
                        deletedCount++;
                    }
                });
            
            Log.Debug($"Deleted {deletedCount} expired files.");
        }

        private Dictionary<string, string> LoadHeaders(string headersFile)
        {
            if (!File.Exists(headersFile))
                return null;

            return (from line in File.ReadAllLines(headersFile)
                    let separatorIndex = GetSeparatorIndex(line)
                    select new
                    {
                        Key = line.Left(separatorIndex).Trim(),
                        Value = line.Substring(separatorIndex + 2).Trim()
                    })
                .ToDictionary(x => x.Key, x => x.Value);
        }

        private bool IsExpired(Dictionary<string, string> headers, DateTime utcNow, DateTime fileDate, TimeSpan defaultExpirySpan)
        {
            var cacheControl = headers.GetValueOrDefault("cache-control");
            if (cacheControl != null)
            {
                var maxAge = GetMaxAge(cacheControl);
                if (maxAge != null)
                {
                    var requestDate = ParseDateTime(headers.GetValueOrDefault("date")) ?? fileDate;
                    return utcNow >= requestDate + maxAge;
                }
            }
            
            var expires = ParseDateTime(headers.GetValueOrDefault("expires")) ?? fileDate + defaultExpirySpan;
            return utcNow >= expires;
        }

        private TimeSpan? GetMaxAge(string cacheControl)
        {
            const string maxAge = "max-age=";
            int startIndex = cacheControl.IndexOf(maxAge, StringComparison.Ordinal);
            if (startIndex == -1)
                return null;

            int endIndex = cacheControl.IndexOf(',', startIndex + maxAge.Length);
            if (endIndex == -1)
                endIndex = cacheControl.Length;

            int length = endIndex - startIndex;
            string secondsStr = cacheControl.Substring(startIndex, length);
            int seconds;
            if (!int.TryParse(secondsStr, out seconds))
                return null;

            return TimeSpan.FromSeconds(seconds);
        }

        private DateTime? ParseDateTime(string str) =>
            str != null
                ? DateTime.ParseExact(str,
                    "ddd, dd MMM yyyy HH:mm:ss 'UTC'",
                    CultureInfo.InvariantCulture.DateTimeFormat,
                    DateTimeStyles.AssumeUniversal)
                : (DateTime?) null;

        public void Save(Uri uri, byte[] bytes, IDictionary<string, string> headers)
        {
            var filePath = GetFilePath(uri);
            Log.Debug($"Save cache to: {filePath}");
            File.WriteAllBytes(filePath, bytes);
            File.WriteAllLines(GetHeadersFile(filePath), headers.Select(x => $"{x.Key.ToLower()}: {x.Value}").ToArray());
        }

        private string GetFilePath(Uri uri) =>
            GetCacheDir() + Path.DirectorySeparatorChar + GetEscapedFileName(uri);

        private string GetHeadersFile(string filePath) =>
            filePath + HeadersExtension;

        private int GetSeparatorIndex(string line)
        {
            var index = line.IndexOf(": ", StringComparison.Ordinal);
            if (index == -1)
                throw new InvalidOperationException("Malformed cache headers file");
            return index;
        }

        private string GetCacheDir()
        {
            if (_cacheDir != null)
                return _cacheDir;
            
            _cacheDir = Application.temporaryCachePath +
                      Path.DirectorySeparatorChar +
                      Application.version +
                      Path.DirectorySeparatorChar +
                      "Silphid.Loadzup.Cache";

            Log.Debug($"Cache path: {_cacheDir}");
            Directory.CreateDirectory(_cacheDir);
            return _cacheDir;
        }

        private string GetEscapedFileName(Uri uri)
        {
            var invalidCharacters = Regex.Escape(new string(Path.GetInvalidFileNameChars()) + ":");
            var regex = new Regex($"[{invalidCharacters}]");
            return regex.Replace(uri.AbsoluteUri, "_");
        }
    }
}