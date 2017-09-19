using System;
using System.Linq;
using System.Text;
using UniRx;
using UnityEngine;

namespace Silphid.Loadzup
{
    public class SpriteConverter : IConverter
    {
        private readonly string[] _imageMediaTypes =
        {
            "image/jpeg",
            "image/png"
        };

        public bool Supports<T>(byte[] bytes, ContentType contentType) =>
            _imageMediaTypes.Contains(contentType.MediaType) && typeof(T) == typeof(DisposableSprite);

        public IObservable<T> Convert<T>(byte[] bytes, ContentType contentType, Encoding encoding)
        {
            var texture = new Texture2D(2, 2) {wrapMode = TextureWrapMode.Clamp};
            texture.LoadImage(bytes, true);

            return Observable.Return((T) (object) new DisposableSprite(texture));
        }
    }
}