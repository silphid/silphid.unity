using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Silphid.Options;

namespace Silphid.Showzup
{
    public class JObjectViewOptions : IViewOptions, IOptionsInternal
    {
        private readonly JObject _jObject;

        public JObjectViewOptions(JObject jObject)
        {
            _jObject = jObject;
        }

        public bool HasValue(object key) =>
            _jObject[key] != null;

        public object GetValue(object key) => _jObject[key];

        public IEnumerable<object> GetValues(object key)
        {
            var value = (JArray)GetValue(key);
            foreach (var x in value)
                yield return x;
        }

        public IEnumerable<object> Keys =>
            _jObject.Properties()
                    .Select(p => p.Name)
                    .ToList();
    }
}