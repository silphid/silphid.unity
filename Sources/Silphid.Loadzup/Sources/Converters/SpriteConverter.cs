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

        public bool Supports<T>(object input, ContentType contentType) =>
            (input is Texture2D || input is byte[]) &&
            typeof(T) == typeof(DisposableSprite) &&
            _imageMediaTypes.Contains(contentType.MediaType);

        public IObservable<T> Convert<T>(object input, ContentType contentType, Encoding encoding)
        {
            Texture2D texture;
            bool shouldDisposeTexture;
            
            if (input is Texture2D)
            {
                texture = (Texture2D) input;
                shouldDisposeTexture = false;
            }
            else
            {
                texture = new Texture2D(2, 2) {wrapMode = TextureWrapMode.Clamp};
                texture.LoadImage((byte[]) input, true);
                shouldDisposeTexture = true;
            }
            
            return Observable.Return((T) (object) new DisposableSprite(texture, shouldDisposeTexture));
        }
    }
}