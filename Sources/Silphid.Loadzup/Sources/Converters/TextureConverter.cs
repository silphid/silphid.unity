using System;
using System.Text;
using UniRx;
using UnityEngine;

namespace Silphid.Loadzup
{
    public class TextureConverter : ConverterBase<byte[]>
    {
        public TextureConverter() : base("image/png", "image/jpeg", "application/octet-stream")
        {
        }

        protected override IObservable<T> ConvertInternal<T>(byte[] input, ContentType contentType, Encoding encoding)
        {
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false, false);
            texture.LoadImage(input);
            return Observable.Return((T)(object)texture);
        }
    }
}