namespace Silphid.Showzup.Components
{
    public class HorizontalListLayout : ListLayout
    {
        protected override IListLayout CreateLayout() =>
            new Showzup.HorizontalListLayout(ItemPadding, ItemSize, ItemSpacing, ContainerPadding);
    }
}