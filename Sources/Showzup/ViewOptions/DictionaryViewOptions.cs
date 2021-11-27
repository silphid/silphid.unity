using System.Collections;
using System.Collections.Generic;
using Silphid.Options;

namespace Silphid.Showzup
{
    public class DictionaryViewOptions : IViewOptions, IOptionsInternal
    {
        private readonly IDictionary<object, object> _dictionary;

        public DictionaryViewOptions(IDictionary<object, object> dictionary)
        {
            _dictionary = dictionary;
        }

        public bool HasValue(object key) =>
            _dictionary.ContainsKey(key);

        public object GetValue(object key)
        {
            _dictionary.TryGetValue(key, out var value);
            return value;
        }

        public IEnumerable<object> GetValues(object key)
        {
            var value = GetValue(key);
            if (!(value is string) && value is IEnumerable enumerable)
                foreach (var x in enumerable)
                    yield return x;
            else if (value != null)
                yield return value;
        }

        public IEnumerable<object> Keys =>
            _dictionary.Keys;
    }
}