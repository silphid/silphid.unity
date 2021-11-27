using UnityEngine;

namespace Silphid.Showzup.Virtual.Coordinates
{
    public interface ITransformer
    {
        Rect TransformRect(Rect value);
        Rect InverseTransformRect(Rect value);
        
        Vector2 TransformPoint(Vector2 value);
        Vector2 InverseTransformPoint(Vector2 value);

        Vector2 TransformSize(Vector2 value);
        Vector2 InverseTransformSize(Vector2 value);
    }
}