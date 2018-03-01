using System.Text;
using UnityEngine;

namespace Silphid.Loadzup
{
    public class JsonUtilityConverter : ConverterBase<byte[]>
    {
        public JsonUtilityConverter()
        {
            SetMediaTypes(KnownMediaType.ApplicationJson);
        }

        protected override object ConvertSync<T>(byte[] input, ContentType contentType, Encoding encoding) =>
            JsonUtility.FromJson<T>(encoding.GetString(input));
    }
}