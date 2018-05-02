using System;
using System.Collections.Generic;
using System.Text;
using Silphid.Extensions;
using Silphid.Loadzup.Http;
using UnityEngine;

namespace Silphid.Loadzup
{
    public class Response
    {
        private readonly Options _options;
        private ContentType _contentType;
        private Encoding _encoding;

        public long StatusCode { get; }
        public byte[] Bytes { get; }
        public Texture2D Texture { get; }
        public readonly Dictionary<string, string> Headers;

        public Response(long statusCode, byte[] bytes, IDictionary<string, string> headers, Options options = null,
            Texture2D texture = null)
        {
            _options = options;
            StatusCode = statusCode;
            Bytes = bytes;
            Texture = texture;
            if (headers != null)
                Headers = new Dictionary<string, string>(headers, StringComparer.OrdinalIgnoreCase);
        }

        public ContentType ContentType
        {
            get
            {
                if (_options?.ContentType != null)
                    return _options.ContentType;
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