using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Silphid.Loadzup
{
    public class DisposableSprite : IDisposable
    {
        private readonly Texture2D _texture;
        private readonly bool _shouldDisposeTexture;

        public DisposableSprite(Texture2D texture, bool shouldDisposeTexture)
        {
            _texture = texture;
            _shouldDisposeTexture = shouldDisposeTexture;
            Sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }

        public Sprite Sprite { get; }

        public void Dispose()
        {
            if (_shouldDisposeTexture)
                Object.Destroy(_texture);
        }
    }
}