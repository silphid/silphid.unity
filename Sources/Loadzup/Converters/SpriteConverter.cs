using System.Text;
using UnityEngine;

namespace Silphid.Loadzup
{
    public class SpriteConverter : ConverterBase<Texture2D, byte[]>
    {
        public SpriteConverter()
        {
            SetMediaTypes("image/jpeg", "image/png");
            SetOutput<DisposableSprite>();
        }

        protected override bool SupportsInternal<T>(object input, string mediaType) =>
            typeof(T) == typeof(DisposableSprite);

        protected override object ConvertSync<T>(Texture2D input, string mediaType, Encoding encoding) =>
            new DisposableSprite(input, false);

        protected override object ConvertSync<T>(byte[] input, string mediaType, Encoding encoding)
        {
            var texture = new Texture2D(2, 2) { wrapMode = TextureWrapMode.Clamp };

            texture.LoadImage(input, true);
            return new DisposableSprite(texture, true);
        }
    }
}