namespace Silphid.Showzup.ListLayouts.Components
{
    public class HorizontalListLayout : ListLayout
    {
        protected override IListLayout CreateLayout() =>
            new ListLayouts.HorizontalListLayout(ItemPadding, ItemSize, ItemSpacing, ContainerPadding);
    }
}