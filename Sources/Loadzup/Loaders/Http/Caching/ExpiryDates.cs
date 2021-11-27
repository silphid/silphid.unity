using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using log4net;

namespace Silphid.Loadzup.Http.Caching
{
    public class ExpiryDates
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ExpiryDates));

        public static readonly Regex EntryRegex = new Regex(
            @"^(?<Name>\w+)\s*:\s*(?<Value>.*)$",
            RegexOptions.Multiline);

        private readonly string _path;
        private readonly IDictionary<string, DateTime> _entries;

        private ExpiryDates(string path, IDictionary<string, DateTime> entries)
        {
            _path = path;
            _entries = entries;
        }

        public static ExpiryDates Load(string path)
        {
            var entries = new Dictionary<string, DateTime>();

            try
            {
                if (File.Exists(path))
                {
                    var text = File.ReadAllText(path);
                    foreach (Match match in EntryRegex.Matches(text))
                    {
                        var name = match.Groups["Name"]
                                        .Value;
                        var value = match.Groups["Value"]
                                         .Value;
                        entries[name] = DateTime.Parse(value);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Failed to load cache expiry dates from file: {path}", ex);
            }

            return new ExpiryDates(path, entries);
        }

        public void Save()
        {
            var lines = _entries.Select(x => $"{x.Key}: {x.Value}");
            File.WriteAllLines(_path, lines);
        }

        public bool IsValid(string cacheGroup, DateTime dateTime)
        {
            if (cacheGroup == null)
                return true;

            DateTime expiryDate;
            return !_entries.TryGetValue(cacheGroup, out expiryDate) || dateTime > expiryDate;
        }

        public void Invalidate(string cacheGroup)
        {
            _entries[cacheGroup] = DateTime.UtcNow;
        }
    }
}