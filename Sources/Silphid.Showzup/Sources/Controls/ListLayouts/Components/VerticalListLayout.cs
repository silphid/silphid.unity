namespace Silphid.Showzup.Components
{
    public class VerticalListLayout : ListLayout
    {
        protected override IListLayout CreateLayout() =>
            new Showzup.VerticalListLayout(ItemPadding, ItemSize, ItemSpacing, ContainerPadding);
    }
}