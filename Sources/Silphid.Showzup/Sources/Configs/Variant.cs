using System;
using System.Collections.Generic;
using System.Linq;

namespace Silphid.Showzup
{
    public class Variant
    {
        private const string Wildcard = "*";
        private const string CategoryNameSeparator = "/";
        private static readonly char[] CategoryNameSeparators = { '/' };
        private static readonly char[] MultiNameSeparators = { '+' };
        private static readonly char[] MultiVariantSeparators = { ',', ' ' };

        public string Category { get; }
        public string Name { get; }

        public Variant(string category, string name)
        {
            Category = category;
            Name = name;
        }

        public override string ToString() =>
            Category != null ? $"{Category}{CategoryNameSeparator}{Name ?? Wildcard}" : Name ?? Wildcard;

        public static IEnumerable<Variant> Parse(string value) =>
            value
                .Split(MultiVariantSeparators, StringSplitOptions.RemoveEmptyEntries)
                .SelectMany(ParseItem);

        private static IEnumerable<Variant> ParseItem(string value)
        {
            var tokens = value.Split(CategoryNameSeparators);

            string category;
            string names;

            if (tokens.Length == 1)
            {
                category = null;
                names = tokens[0];
            }
            else if (tokens.Length == 2)
            {
                category = tokens[0];
                names = tokens[1];
            }
            else
                throw new FormatException("Malformed variant: {value}");

            return ParseNames(names)
                .Select(name => new Variant(category, name));
        }

        private static IEnumerable<string> ParseNames(string names) =>
            names
                .Split(MultiNameSeparators)
                .Select(name => name == Wildcard ? null : name);
    }
}