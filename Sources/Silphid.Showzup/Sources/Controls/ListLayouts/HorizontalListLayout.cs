using Silphid.Extensions;
using UnityEngine;

namespace Silphid.Showzup
{
    public class HorizontalListLayout : ListLayout
    {
        public HorizontalListLayout(Vector2 itemPadding, Vector2 itemSize, Vector2 itemSpacing, Vector2 containerPadding) :
            base(itemPadding, itemSize, itemSpacing, containerPadding)
        {
        }

        public override Rect GetItemRect(int index) =>
            new Rect(
                FirstItemPosition + ItemOffset.WithY(0) * index,
                ItemSize);

        public override Vector2 GetContainerSize(int count) =>
            ContainerPadding * 2 +
            new Vector2(
                (ItemSize.x + ItemPadding.x * 2) * count + ItemSpacing.x * (count - 1).Minimum(0),
                ItemSize.y + ItemPadding.y * 2);
    }
}