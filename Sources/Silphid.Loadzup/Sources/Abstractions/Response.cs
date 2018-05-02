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
        private readonly Func<byte[]> _bytesSelector;
        private readonly Func<Texture2D> _textureSelector;
        private readonly Options _options;
        private ContentType _contentType;
        private Encoding _encoding;
        private byte[] _bytes;
        private Texture2D _texture;

        public long StatusCode { get; }
        public byte[] Bytes => _bytes ?? (_bytes = _bytesSelector());
        public Texture2D Texture => _texture ?? (_texture = _textureSelector());
        public readonly Dictionary<string, string> Headers;

        public Response(long statusCode, Func<byte[]> bytesSelector, IDictionary<string, string> headers, Options options = null,
            Func<Texture2D> textureSelector = null)
        {
            _bytesSelector = bytesSelector;
            _options = options;
            _textureSelector = textureSelector;
            StatusCode = statusCode;
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