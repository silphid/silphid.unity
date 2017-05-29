#if JSON_NET

using System.Text;
using Newtonsoft.Json;

namespace Silphid.Loadzup.JsonNet
{
    public class JsonNetConverter : IConverter
    {
        private readonly JsonSerializerSettings _settings;

        public JsonNetConverter(JsonSerializerSettings settings)
        {
            _settings = settings;
        }

        public bool Supports<T>(byte[] bytes, ContentType contentType) =>
            contentType.MediaType == KnownMediaType.ApplicationJson;

        public T Convert<T>(byte[] bytes, ContentType contentType, Encoding encoding) =>
            (T)JsonConvert.DeserializeObject(encoding.GetString(bytes), typeof(T), _settings);
    }
}

#endif