using System;
using System.Collections.Generic;
using System.Text;
using Silphid.Loadzup.Http;
using UnityEngine;

namespace Silphid.Loadzup
{
    public class Response
    {
        private readonly Func<byte[]> _bytesSelector;
        private readonly Func<Texture2D> _textureSelector;
        private Encoding _encoding;
        private byte[] _bytes;
        private Texture2D _texture;

        public long StatusCode { get; }
        public byte[] Bytes => _bytes ?? (_bytes = _bytesSelector());

        public Texture2D Texture => _texture
                                        ? _texture
                                        : _textureSelector != null
                                            ? _texture = _textureSelector()
                                            : null;

        public readonly Headers Headers;

        public Response(long statusCode,
                        IDictionary<string, string> headers,
                        Func<byte[]> bytesSelector,
                        Func<Texture2D> textureSelector = null)
        {
            _bytesSelector = bytesSelector;
            _textureSelector = textureSelector;
            StatusCode = statusCode;
            Headers = Headers.From(headers);
        }

        public Response(long statusCode,
                        Headers headers,
                        Func<byte[]> bytesSelector,
                        Func<Texture2D> textureSelector = null)
        {
            _bytesSelector = bytesSelector;
            _textureSelector = textureSelector;
            StatusCode = statusCode;
            Headers = headers;
        }

        public Encoding Encoding =>
            _encoding ?? (_encoding = Headers.ContentType?.CharSet != null
                                          ? Encoding.GetEncoding(Headers.ContentType.CharSet)
                                          : Encoding.UTF8);
    }
}