using System.Text;
using UnityEngine;

namespace Silphid.Loadzup
{
    public class JsonConverter : ConverterBase<byte[]>
    {
        public JsonConverter()
        {
            SetMediaTypes(KnownMediaType.ApplicationJson);
        }

        protected override object ConvertSync<T>(byte[] input, ContentType contentType, Encoding encoding) =>
            JsonUtility.FromJson<T>(encoding.GetString(input));
    }
}