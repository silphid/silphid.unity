using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace Silphid.Loadzup
{
    public class XmlConverter : ConverterBase<byte[]>
    {
        public XmlConverter()
        {
            SetMediaTypes(KnownMediaType.ApplicationXml, KnownMediaType.TextXml);
        }

        protected override object ConvertSync<T>(byte[] input, ContentType contentType, Encoding encoding)
        {
            var serializer = new XmlSerializer(typeof(T));
            using (var stream = new MemoryStream(input))
                return serializer.Deserialize(stream);
        }
    }
}