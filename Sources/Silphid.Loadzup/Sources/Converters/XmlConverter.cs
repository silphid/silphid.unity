using System;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using UniRx;

namespace Silphid.Loadzup
{
    public class XmlConverter : ConverterBase<byte[]>
    {
        public XmlConverter() : base(KnownMediaType.ApplicationXml, KnownMediaType.TextXml)
        {
        }

        protected override IObservable<T> ConvertInternal<T>(byte[] input, ContentType contentType, Encoding encoding)
        {
            var serializer = new XmlSerializer(typeof(T));
            using (var stream = new MemoryStream(input))
                return Observable.Return((T) serializer.Deserialize(stream));
        }
    }
}