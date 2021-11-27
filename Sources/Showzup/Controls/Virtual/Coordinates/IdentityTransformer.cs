using UnityEngine;

namespace Silphid.Showzup.Virtual.Coordinates
{
    public class IdentityTransformer : ITransformer
    {
        public Rect TransformRect(Rect value) => value;
        public Rect InverseTransformRect(Rect value) => value;
        public Vector2 TransformPoint(Vector2 value) => value;
        public Vector2 InverseTransformPoint(Vector2 value) => value;
        public Vector2 TransformSize(Vector2 value) => value;
        public Vector2 InverseTransformSize(Vector2 value) => value;
    }
}