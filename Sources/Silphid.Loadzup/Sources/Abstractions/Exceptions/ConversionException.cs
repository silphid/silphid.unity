using System;
using System.Text;
using Silphid.Extensions;

namespace Silphid.Loadzup
{
    public class ConversionException : LoadzupException
    {
        public Type ConverterType { get; }
        public object Input { get; }
        public Type OutputType { get; }
        public ContentType ContentType { get; }
        public Encoding Encoding { get; }

        public ConversionException(Type converterType, object input, Type outputType, ContentType contentType, Encoding encoding)
        {
            ConverterType = converterType;
            Input = input;
            OutputType = outputType;
            ContentType = contentType;
            Encoding = encoding;
        }

        public ConversionException(string message, Type converterType, object input, Type outputType, ContentType contentType, Encoding encoding) : base(message)
        {
            ConverterType = converterType;
            Input = input;
            OutputType = outputType;
            ContentType = contentType;
            Encoding = encoding;
        }

        public ConversionException(string message, Exception innerException, Type converterType, object input, Type outputType, ContentType contentType, Encoding encoding) : base(message, innerException)
        {
            ConverterType = converterType;
            Input = input;
            OutputType = outputType;
            ContentType = contentType;
            Encoding = encoding;
        }

        public override string Message =>
            $"{base.Message}\r\n" +
            $"Converter: {ConverterType.Name}\r\n" +
            $"Input: {FormatInput()}\r\n" +
            $"OutputType: {OutputType.Name}\r\n" +
            $"ContentType: {ContentType}\r\n" +
            $"Encoding: {Encoding?.WebName}\r\n";

        private string FormatInput()
        {
            const int MaxLength = 100;
            
            if (Input == null)
                return "NULL";
            
            if (Input is byte[] && Encoding != null)
                return Encoding.GetString((byte[]) Input).WithEllipsis(MaxLength);

            if (Input is string)
                return ((string) Input).WithEllipsis(MaxLength);

            return Input.ToString();
        }
    }
}