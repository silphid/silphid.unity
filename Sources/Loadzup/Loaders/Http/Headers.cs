using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using Silphid.Extensions;
using UniRx;

namespace Silphid.Loadzup.Http
{
    public class Headers
    {
        private static readonly Regex HeaderRegex = new Regex(
            @"^(?<Key>[\w-]+)\s*:\s*(?<Value>.*)$",
            RegexOptions.Multiline);

        private readonly IDictionary<string, string> Dictionary;
        private CacheControlHeaderValue _cacheControl;
        private MediaTypeHeaderValue _mediaType;
        private DateTimeHeaderValue _lastModified;
        private DateTimeHeaderValue _date;

        public Headers()
        {
            Dictionary = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        internal Headers(IDictionary<string, string> dictionary)
        {
            Dictionary = dictionary;
        }

        public static Headers From(IDictionary<string, string> dictionary)
        {
            return dictionary != null
                       ? new Headers(new Dictionary<string, string>(dictionary, StringComparer.OrdinalIgnoreCase))
                       : new Headers();
        }

        public static Headers Load(string path) =>
            Parse(File.ReadAllText(path));

        public void Save(string path) =>
            Observable.Start(() => File.WriteAllText(path, ToString()))
                      .Subscribe();

        public static Headers Parse(string text) =>
            new Headers(
                HeaderRegex.Matches(text)
                           .Cast<Match>()
                           .ToDictionary(
                                x => x.Groups["Key"]
                                      .Value,
                                x => x.Groups["Value"]
                                      .Value,
                                StringComparer.OrdinalIgnoreCase));

        public string this[string key]
        {
            get
            {
                string value;
                if (Dictionary.TryGetValue(key, out value))
                    return value;

                return null;
            }
            set { Dictionary[key] = value; }
        }

        public CacheControlHeaderValue CacheControl
        {
            get
            {
                if (_cacheControl == null)
                {
                    string value;
                    if (Dictionary.TryGetValue(KnownHeaders.CacheControl, out value))
                        _cacheControl = CacheControlHeaderValue.Parse(value);
                }

                return _cacheControl;
            }
            set
            {
                _cacheControl = value;
                Dictionary[KnownHeaders.CacheControl] = _cacheControl.ToString();
            }
        }

        public MediaTypeHeaderValue ContentType
        {
            get
            {
                if (_mediaType == null)
                {
                    string value;
                    if (Dictionary.TryGetValue(KnownHeaders.ContentType, out value))
                        _mediaType = MediaTypeHeaderValue.Parse(value);
                }

                return _mediaType;
            }
            set
            {
                _mediaType = value;
                Dictionary[KnownHeaders.ContentType] = _mediaType.ToString();
            }
        }

        public string ETag
        {
            get { return this[KnownHeaders.ETag]; }
            set { this[KnownHeaders.ETag] = value; }
        }

        public string E2CacheGroup
        {
            get { return this["E2-Cache-Group"]; }
            set { this["E2-Cache-Group"] = value; }
        }

        public DateTimeHeaderValue LastModified
        {
            get
            {
                if (_lastModified == null)
                {
                    string text;
                    if (Dictionary.TryGetValue(KnownHeaders.ContentType, out text))
                        _lastModified = DateTimeHeaderValue.Parse(text);
                }

                return _lastModified;
            }
            set { _lastModified = value; }
        }

        public DateTimeHeaderValue Date
        {
            get
            {
                if (_date == null)
                {
                    string text;
                    if (Dictionary.TryGetValue(KnownHeaders.Date, out text))
                        _date = DateTimeHeaderValue.Parse(text);
                }

                return _date;
            }
            set { _date = value; }
        }

        public override string ToString() =>
            Dictionary.Select(ReplaceWithOverrides)
                      .Select(x => $"{x.Key}: {x.Value}")
                      .JoinAsString(Environment.NewLine);

        private KeyValuePair<string, string> ReplaceWithOverrides(KeyValuePair<string, string> pair)
        {
            if (_cacheControl != null && pair.Key.CaseInsensitiveEquals(KnownHeaders.CacheControl))
                return new KeyValuePair<string, string>(KnownHeaders.CacheControl, _cacheControl.ToString());

            if (_mediaType != null && pair.Key.CaseInsensitiveEquals(KnownHeaders.ContentType))
                return new KeyValuePair<string, string>(KnownHeaders.ContentType, _mediaType.ToString());

            if (_lastModified != null && pair.Key.CaseInsensitiveEquals(KnownHeaders.LastModified))
                return new KeyValuePair<string, string>(KnownHeaders.LastModified, _lastModified.ToString());

            return pair;
        }
    }
}