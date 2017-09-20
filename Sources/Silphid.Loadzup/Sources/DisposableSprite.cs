using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Silphid.Loadzup
{
    public class DisposableSprite : IDisposable
    {
        private readonly Texture2D _texture;

        public DisposableSprite(Texture2D texture)
        {
            _texture = texture;
            Sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }

        public Sprite Sprite { get; }

        public void Dispose()
        {
            Object.Destroy(_texture);
        }
    }
}