namespace Silphid.Showzup.ListLayouts.Components
{
    public class VerticalListLayout : ListLayout
    {
        protected override IListLayout CreateLayout() =>
            new ListLayouts.VerticalListLayout(ItemPadding, ItemSize, ItemSpacing, ContainerPadding);
    }
}