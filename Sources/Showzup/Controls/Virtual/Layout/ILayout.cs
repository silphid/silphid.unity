using Silphid.DataTypes;
using UnityEngine;

namespace Silphid.Showzup.Virtual.Layout
{
    public interface ILayout
    {
        (float requiredHeight, float adjustment) Perform(LayoutDirection direction,
                                                         ILayoutCollection collection,
                                                         Rect viewportRect,
                                                         Vector2 availableSize);

        IntRange GetActiveRange(ILayoutCollection collection, Rect viewportRect, Vector2 availableSize);
    }
}