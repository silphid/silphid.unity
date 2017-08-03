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
            _imageMediaTypes.Contains(contentType.MediaType);

        public IObservable<T> Convert<T>(byte[] bytes, ContentType contentType, Encoding encoding)
        {
            var texture = new Texture2D(2, 2);
            texture.LoadImage(bytes);

            return Observable.Return((T)(object)Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f)));
        }
    }
}