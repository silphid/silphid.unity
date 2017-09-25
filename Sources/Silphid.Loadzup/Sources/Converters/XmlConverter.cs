using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using UniRx;

namespace Silphid.Loadzup
{
    public class XmlConverter : IConverter
    {
        private readonly string[] _xmlMediaTypes =
        {
            KnownMediaType.ApplicationXml,
            "text/xml"
        };
        
        public bool Supports<T>(object input, ContentType contentType) =>
            input is byte[] && _xmlMediaTypes.Contains(contentType.MediaType);

        public IObservable<T> Convert<T>(object input, ContentType contentType, Encoding encoding)
        {
            var serializer = new XmlSerializer(typeof(T));
            using (var stream = new MemoryStream((byte[]) input))
                return Observable.Return((T) serializer.Deserialize(stream));
        }
    }
}