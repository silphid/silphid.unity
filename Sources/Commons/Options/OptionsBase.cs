using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Silphid.DataTypes;

namespace Silphid.Options
{
    public abstract class OptionsBase : IOptionsInternal
    {
        private readonly IOptionsInternal _innerOptions;
        private readonly object _key;
        private readonly object _value;

        protected OptionsBase(IOptionsBase innerOptions, object key, object value)
        {
            _innerOptions = (IOptionsInternal) innerOptions;
            _key = key;
            _value = value;
        }

        public bool HasValue(object key)
        {
            if (_key == key)
                return true;

            return _innerOptions?.HasValue(key) ?? false;
        }

        public object GetValue(object key)
        {
            if (key == _key)
                return _value;

            return _innerOptions?.GetValue(key);
        }

        public IEnumerable<object> GetValues(object key)
        {
            if (_innerOptions != null)
                foreach (var x in _innerOptions.GetValues(key))
                    yield return x;

            if (key == _key)
            {
                if (!(_value is string) && _value is IEnumerable enumerable)
                {
                    foreach (var x in enumerable)
                        yield return x;
                }
                else
                    yield return _value;
            }
        }

        public IEnumerable<object> Keys
        {
            get
            {
                if (_innerOptions != null)
                    foreach (var x in _innerOptions.GetKeys())
                        yield return x;

                yield return _key;
            }
        }

        public override string ToString()
        {
            var builder = new StringBuilder();

            foreach (var key in Keys)
            {
                if (builder.Length > 0)
                    builder.Append(", ");

                var value = GetValue(key);

                builder.Append(GetFormattedKeyValue(key, value));
            }

            return builder.ToString();
        }

        private string GetFormattedKeyValue(object key, object value) =>
            $"{GetFormattedKey(key)}: {GetFormattedValue(value)}";

        private string GetFormattedKey(object key) =>
            key is Type type
                ? type.IsGenericType
                      ? GetGenericTypeName(type)
                      : type.Name
                : key.ToString();

        private string GetGenericTypeName(Type type) =>
            type.GetGenericArguments()
                .First()
                .Name +
            (type.GetGenericTypeDefinition() == typeof(Nullable<>)
                 ? "?"
                 : "");

        private string GetFormattedValue(object value)
        {
            if (!(value is string) && value is IEnumerable enumerable)
            {
                var builder = new StringBuilder();
                builder.Append("[");

                bool isFirst = true;
                foreach (var item in enumerable)
                {
                    if (!isFirst)
                        builder.Append(", ");

                    var formattedItem = GetFormattedValue(item);

                    builder.Append(formattedItem);
                    isFirst = false;
                }

                builder.Append("]");
                return builder.ToString();
            }

            if (value is DictionaryEntry entry)
                return GetFormattedKeyValue(entry.Key, entry.Value);

            if (IsPrimitive(value))
                return value.ToString();

            return $"{{{value}}}";
        }

        private static bool IsPrimitive(object value) =>
            value is string ||
            value is ITypeSafeEnum ||
            value.GetType()
                 .IsEnum ||
            value.GetType()
                 .IsPrimitive;
    }
}