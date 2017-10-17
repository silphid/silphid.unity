using System;
using System.Collections.Generic;
using System.Globalization;
using Silphid.Extensions;

namespace Silphid.Loadzup.Http.Caching
{
    /// <summary>
    /// Facilitates parsing value of "Cache-Control" HTTP header.
    /// </summary>
    public class CacheControl
    {
        public DateTime Expiry { get; }
        
        public CacheControl(Dictionary<string, string> headers, DateTime fileDate, TimeSpan defaultExpirySpan)
        {
            Expiry = GetExpiry(headers, fileDate, defaultExpirySpan);
        }

        private DateTime GetExpiry(Dictionary<string, string> headers, DateTime fileDate, TimeSpan defaultExpirySpan)
        {
            var date = ParseDateTime(headers.GetValueOrDefault("date")) ?? fileDate;
            
            var cacheControl = headers.GetValueOrDefault(KnownHttpHeaders.CacheControl);
            if (cacheControl != null)
            {
                var maxAge = GetMaxAge(cacheControl);
                if (maxAge != null)
                    return date + maxAge.Value;
            }

            return ParseDateTime(headers.GetValueOrDefault("expires")) ?? date + defaultExpirySpan;
        }

        private TimeSpan? GetMaxAge(string cacheControl)
        {
            const string MaxAge = "max-age=";
            int startIndex = cacheControl.IndexOf(MaxAge, StringComparison.Ordinal);
            if (startIndex == -1)
                return null;
            startIndex += MaxAge.Length;

            int endIndex = cacheControl.IndexOf(',', startIndex);
            if (endIndex == -1)
                endIndex = cacheControl.Length;

            int length = endIndex - startIndex;
            string secondsStr = cacheControl.Substring(startIndex, length);
            int seconds;
            if (!int.TryParse(secondsStr, out seconds))
                return null;

            return TimeSpan.FromSeconds(seconds);
        }

        private DateTime? ParseDateTime(string str)
        {
            DateTime dateTime;
            if (DateTime.TryParseExact(str, "ddd, dd MMM yyyy HH:mm:ss 'GMT'",
                CultureInfo.InvariantCulture.DateTimeFormat,
                DateTimeStyles.AssumeUniversal, out dateTime))
                return dateTime.ToUniversalTime();

            return null;
        }
    }
}