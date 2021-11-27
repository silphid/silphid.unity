using Silphid.Extensions;
using UnityEngine;

namespace Silphid.Showzup.Virtual.Coordinates
{
    public class FlippedYTransformer : ITransformer
    {
        private readonly ITransformer _inner;

        public FlippedYTransformer(ITransformer inner)
        {
            _inner = inner;
        }

        public Rect TransformRect(Rect value) => _inner.TransformRect(value)
                                                   .FlippedY();

        public Rect InverseTransformRect(Rect value) => _inner.InverseTransformRect(value.FlippedY());

        public Vector2 TransformPoint(Vector2 value) => _inner.TransformPoint(value)
                                                              .FlippedY();

        public Vector2 InverseTransformPoint(Vector2 value) => _inner.InverseTransformPoint(value.FlippedY());

        public Vector2 TransformSize(Vector2 value) => _inner.TransformSize(value);

        public Vector2 InverseTransformSize(Vector2 value) => _inner.InverseTransformSize(value);
    }
}