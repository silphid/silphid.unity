using System;
using System.Linq;
using Silphid.Extensions;

namespace Silphid.Loadzup
{
    public class ContentType
    {
        public string Type { get; }
        public string SubType { get; }
        public string MediaType => $"{Type}/{SubType}";
        public string Name { get; private set; }
        public string CharSet { get; private set; }
        public string Boundary { get; private set; }

        public ContentType(string value)
        {
            // Parse first level
            var tokens = value
                .Split(';')
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrEmpty(x))
                .ToArray();

            if (tokens.Length == 0)
                throw new FormatException($"Invalid content type: {value}");

            // Parse type and sub-type
            var typeAndSubType = tokens[0]
                .Split('/')
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrEmpty(x))
                .ToArray();

            if (typeAndSubType.Length != 2)
                throw new FormatException($"Invalid content type: {value}");

            Type = typeAndSubType[0];
            SubType = typeAndSubType[1];

            // Parse parameters
            tokens.Skip(1).ForEach(ParseParameter);
        }

        private void ParseParameter(string parameter)
        {
            var tokens = parameter
                .Split('=')
                .Select(x => x.Trim())
                .Where(x => !string.IsNullOrEmpty(x))
                .ToArray();

            if (tokens.Length != 2)
                throw new FormatException($"Invalid content type parameter: {parameter}");

            var key = tokens[0];
            var value = tokens[1];

            if (key == "name")
                Name = value;
            else if (key == "charset")
                CharSet = value;
            else if (key == "boundary")
                Boundary = value;
        }

        public override string ToString() => Name;
    }
}