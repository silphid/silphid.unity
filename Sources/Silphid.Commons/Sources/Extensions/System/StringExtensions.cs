using System;
using System.Globalization;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Silphid.Extensions
{
    public static class StringExtensions
    {
        private const string Ellipsis = "...";
        
        public static string WithEllipsis(this string This, int maxLength) =>
            This.Length <= maxLength
                ? This
                : This.Left(maxLength - Ellipsis.Length) + Ellipsis;

        public static string ToStringInvariant(this float obj)
        {
            return obj.ToString(CultureInfo.InvariantCulture);
        }

        public static string ToStringInvariant(this double obj)
        {
            return obj.ToString(CultureInfo.InvariantCulture);
        }

        public static string ToStringInvariant(this int obj)
        {
            return obj.ToString(CultureInfo.InvariantCulture);
        }

        public static string ToStringInvariant(this long obj)
        {
            return obj.ToString(CultureInfo.InvariantCulture);
        }

        public static bool CaseInsensitiveEquals(this string str1, string str2)
        {
            return string.Compare(str1, str2, StringComparison.OrdinalIgnoreCase) == 0;
        }

        public static bool CaseInsensitiveContains(this string str1, string str2)
        {
            return str1.IndexOf(str2, StringComparison.OrdinalIgnoreCase) != -1;
        }

        [StringFormatMethod("format")]
        public static string Formatted(this string format, params object[] args)
        {
            return format == null ? string.Empty : string.Format(CultureInfo.InvariantCulture, format, args);
        }

        public static string Capitalize(this string value)
        {
            if (value.Length == 0)
                return value;

            if (value.Length == 1)
                return value.ToUpper();

            return value.Substring(0, 1).ToUpper() + value.Substring(1).ToLower();
        }

        public static string ToUpperFirst(this string value)
        {
            if (value.Length == 0)
                return value;

            if (value.Length == 1)
                return value.ToUpper();

            return value.Substring(0, 1).ToUpper() + value.Substring(1);
        }

        public static string ToLowerFirst(this string value)
        {
            if (value.Length == 0)
                return value;

            if (value.Length == 1)
                return value.ToLower();

            return value.Substring(0, 1).ToLower() + value.Substring(1);
        }

        public static string Remove(this string value, params string[] strings)
        {
            foreach (var str in strings)
            {
                value = value.Replace(str, "");
            }

            return value;
        }

        public static string RemovePrefix(this string value, string prefix)
        {
            if (value.StartsWith(prefix))
                return value.RemoveLeft(prefix.Length);

            return value;
        }

        public static string RemoveSuffix(this string value, string suffix)
        {
            if (value.EndsWith(suffix))
                return value.RemoveRight(suffix.Length);

            return value;
        }

        public static string Prepend(this string value, string prefix) =>
            prefix + value;

        public static string Append(this string value, string suffix) =>
            value + suffix;

        public static string AddPrefixIfAbsent(this string value, string prefix) =>
            value.StartsWith(prefix) ? value : prefix + value;

        public static string AddSuffixIfAbsent(this string value, string suffix) =>
            value.EndsWith(suffix) ? value : value + suffix;

        public static string RemoveLeft(this string value, int count)
        {
            return value.Substring(count.Clamp(0, value.Length));
        }

        public static string RemoveRight(this string value, int count)
        {
            return value.Substring(0, value.Length - count.AtMost(value.Length));
        }

        public static string Left(this string value, int count)
        {
            return value.Substring(0, count.AtMost(value.Length));
        }

        public static string Right(this string value, int count)
        {
            return value.Substring(value.Length - count.AtMost(value.Length));
        }

        public static bool IsNullOrEmpty(this string value)
        {
            return string.IsNullOrEmpty(value);
        }

        public static bool IsNullOrWhiteSpace(this string value)
        {
            return value == null || value.IsNullOrEmpty() || value.Trim().IsNullOrEmpty();
        }

        public static bool Matches(this string value, string regexPattern)
        {
            return new Regex(regexPattern).IsMatch(value);
        }
    }
}