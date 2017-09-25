using System;
using System.Linq;
using System.Text;
using UniRx;
using UnityEngine;

namespace Silphid.Loadzup
{
    public class TextureConverter : IConverter
    {
        private readonly string[] _imageMediaTypes =
        {
            "image/png",
            "image/jpeg",
            "application/octet-stream"
        };

        public bool Supports<T>(object input, ContentType contentType) =>
            input is byte[] && _imageMediaTypes.Contains(contentType.MediaType);

        public IObservable<T> Convert<T>(object input, ContentType contentType, Encoding encoding)
        {
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false, false);
            texture.LoadImage((byte[]) input);
            return Observable.Return((T)(object)texture);
        }
    }
}