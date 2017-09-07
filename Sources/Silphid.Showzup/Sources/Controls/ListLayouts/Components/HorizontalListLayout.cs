namespace Silphid.Showzup.ListLayouts.Components
{
    public class HorizontalListLayout : ListLayout
    {
        protected override IListLayout CreateLayout() =>
            new ListLayouts.HorizontalListLayout(ItemSize, Spacing, Padding);
    }
}