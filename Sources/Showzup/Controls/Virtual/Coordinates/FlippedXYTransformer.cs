using Silphid.Extensions;
using UnityEngine;

namespace Silphid.Showzup.Virtual.Coordinates
{
    public class FlippedXYTransformer : ITransformer
    {
        private readonly ITransformer _inner;

        public FlippedXYTransformer(ITransformer inner)
        {
            _inner = inner;
        }

        public Rect TransformRect(Rect value) => _inner.TransformRect(value)
                                                   .FlippedXY();

        public Rect InverseTransformRect(Rect value) => _inner.InverseTransformRect(value.FlippedXY());

        public Vector2 TransformPoint(Vector2 value) => _inner.TransformPoint(value)
                                                              .FlippedXY();

        public Vector2 InverseTransformPoint(Vector2 value) => _inner.InverseTransformPoint(value.FlippedXY());

        public Vector2 TransformSize(Vector2 value) => _inner.TransformSize(value)
                                                             .FlippedXY();

        public Vector2 InverseTransformSize(Vector2 value) => _inner.InverseTransformSize(value.FlippedXY());
    }
}