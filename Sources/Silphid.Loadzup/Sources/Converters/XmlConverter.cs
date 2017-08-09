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
        
        public bool Supports<T>(byte[] bytes, ContentType contentType) =>
            _xmlMediaTypes.Contains(contentType.MediaType);

        public IObservable<T> Convert<T>(byte[] bytes, ContentType contentType, Encoding encoding)
        {
            var serializer = new XmlSerializer(typeof(T));
            using (var stream = new MemoryStream(bytes))
                return Observable.Return((T) serializer.Deserialize(stream));
        }
    }
}