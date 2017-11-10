using System.Text;
using UnityEngine;

namespace Silphid.Loadzup
{
    public class TextureConverter : ConverterBase<byte[]>
    {
        public TextureConverter()
        {
            SetMediaTypes("image/png", "image/jpeg", "application/octet-stream");
            SetOutput<Texture2D>();
        }

        protected override object ConvertSync<T>(byte[] input, ContentType contentType, Encoding encoding)
        {
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false, false)
            {
                wrapMode = TextureWrapMode.Clamp
            };
            
            texture.LoadImage(input);
            return texture;
        }
    }
}