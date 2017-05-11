using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Silphid.Extensions;
using UnityEngine;

namespace Silphid.Loadzup.Caching
{
    public class CacheStorage : ICacheStorage
    {
        public void Clear()
        {
            Directory
                .GetFiles(GetCacheDir())
                .ForEach(File.Delete);
        }

        public bool Contains(Uri uri) => File.Exists(GetFilePath(uri));

        public byte[] Load(Uri uri) =>
            File.ReadAllBytes(GetFilePath(uri));

        public Dictionary<string, string> LoadHeaders(Uri uri)
        {
            var headersFile = GetHeadersFile(GetFilePath(uri));
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

        public void Save(Uri uri, byte[] bytes, IDictionary<string, string> headers)
        {
            var filePath = GetFilePath(uri);
            Debug.Log($"#Loadzup# Save cache to: {filePath}");
            File.WriteAllBytes(filePath, bytes);
            File.WriteAllLines(GetHeadersFile(filePath), headers.Select(x => $"{x.Key}: {x.Value}").ToArray());
        }

        private string GetFilePath(Uri uri) =>
            GetCacheDir() + Path.DirectorySeparatorChar + GetEscapedFileName(uri);

        private string GetHeadersFile(string filePath) =>
            filePath + ".Headers";

        private int GetSeparatorIndex(string line)
        {
            var index = line.IndexOf(": ", StringComparison.Ordinal);
            if (index == -1)
                throw new InvalidOperationException("Malformed cache headers file");
            return index;
        }

        private string GetCacheDir()
        {
            var path = Application.temporaryCachePath +
                      Path.DirectorySeparatorChar +
                      Application.version +
                      Path.DirectorySeparatorChar +
                      "Silphid.Loadzup.Cache";

            Debug.Log($"#Loadzup# Cache path: {path}");
            Directory.CreateDirectory(path);
            return path;
        }

        private string GetEscapedFileName(Uri uri)
        {
            var invalidCharacters = Regex.Escape(new string(Path.GetInvalidFileNameChars()) + ":");
            var regex = new Regex($"[{invalidCharacters}]");
            return regex.Replace(uri.AbsoluteUri, "_");
        }
    }
}