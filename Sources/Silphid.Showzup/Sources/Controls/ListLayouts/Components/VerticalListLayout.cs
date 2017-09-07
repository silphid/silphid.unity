namespace Silphid.Showzup.ListLayouts.Components
{
    public class VerticalListLayout : ListLayout
    {
        protected override IListLayout CreateLayout() =>
            new ListLayouts.VerticalListLayout(ItemSize, Spacing, Padding);
    }
}