using System;
using System.Globalization;

namespace Silphid.Loadzup.Http
{
    public class DateTimeHeaderValue
    {
        public DateTimeOffset Value { get; }

        public DateTimeHeaderValue(DateTimeOffset value)
        {
            Value = value;
        }

        public static DateTimeHeaderValue Parse(string text) =>
            new DateTimeHeaderValue(
                DateTimeOffset.ParseExact(
                    text,
                    DateTimeFormats,
                    DateTimeFormatInfo.InvariantInfo,
                    DateTimeStyles.AllowWhiteSpaces | DateTimeStyles.AssumeUniversal));

        private static readonly string[] DateTimeFormats =
        {
            "r", "dddd, dd'-'MMM'-'yy HH:mm:ss 'GMT'", "ddd MMM d HH:mm:ss yyyy", "d MMM yy H:m:s",
            "ddd, d MMM yyyy H:m:s zzz"
        };

        public override string ToString() =>
            Value.ToString("r", CultureInfo.InvariantCulture);
    }
}