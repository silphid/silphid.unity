using System;
using System.Text;
using UniRx;
using UnityEngine;

namespace Silphid.Loadzup
{
    public class SpriteConverter : ConverterBase<Texture2D, byte[]>
    {
        public SpriteConverter() : base("image/jpeg", "image/png")
        {
        }

        protected override bool SupportsInternal<T>(object input, ContentType contentType) =>
            typeof(T) == typeof(DisposableSprite);

        protected override IObservable<T> ConvertInternal<T>(Texture2D input, ContentType contentType, Encoding encoding) =>
            Observable.Return((T) (object) new DisposableSprite(input, false));

        protected override IObservable<T> ConvertInternal<T>(byte[] input, ContentType contentType, Encoding encoding)
        {
            var texture = new Texture2D(2, 2) {wrapMode = TextureWrapMode.Clamp};
            texture.LoadImage(input, true);
            return Observable.Return((T) (object) new DisposableSprite(texture, true));
        }
    }
}