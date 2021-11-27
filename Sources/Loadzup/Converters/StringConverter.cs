using System.Text;

namespace Silphid.Loadzup
{
    public class StringConverter : ConverterBase<byte[]>
    {
        public StringConverter()
        {
            SetOutput<string>();
        }

        protected override object ConvertSync<T>(byte[] input, string mediaType, Encoding encoding) =>
            encoding.GetString(input);
    }
}