using System.Text;

namespace Silphid.Loadzup
{
    public class StringConverter : ConverterBase<byte[]>
    {
        public StringConverter()
        {
            SetOutput<string>();
        }

        protected override object ConvertSync<T>(byte[] input, ContentType contentType, Encoding encoding) =>
            encoding.GetString(input);
    }
}