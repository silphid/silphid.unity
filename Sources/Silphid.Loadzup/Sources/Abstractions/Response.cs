using System;
using System.Collections.Generic;
using System.Text;
using Silphid.Extensions;
using Silphid.Loadzup.Http;

namespace Silphid.Loadzup
{
    public class Response
    {
        private ContentType _contentType;
        private Encoding _encoding;

        public long StatusCode { get; }
        public byte[] Bytes { get; }
        public Dictionary<string, string> Headers;

        public Response(long statusCode, byte[] bytes, Dictionary<string, string> headers)
        {
            StatusCode = statusCode;
            Bytes = bytes;
            Headers = new Dictionary<string, string>(headers, StringComparer.OrdinalIgnoreCase);
        }

        public ContentType ContentType
        {
            get
            {
                if (_contentType != null)
                    return _contentType;

                var str = Headers.GetValueOrDefault(KnownHttpHeaders.ContentType);
                if (str != null)
                    _contentType = new ContentType(str);

                return _contentType;
            }
        }

        public Encoding Encoding =>
            _encoding ?? (_encoding = !string.IsNullOrEmpty(ContentType?.CharSet)
                ? Encoding.GetEncoding(ContentType.CharSet)
                : Encoding.UTF8);
    }
}