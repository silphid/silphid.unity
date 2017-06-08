using Silphid.Showzup;

public class Granularity : Variant<Granularity>
{
    public static readonly Granularity Page = new Granularity();
    public static readonly Granularity Panel = new Granularity();
    public static readonly Granularity Tile = new Granularity();
    public static readonly Granularity Thumbnail = new Granularity();
    public static readonly Granularity Item = new Granularity();

    public new static readonly VariantGroup<Granularity> Group = new VariantGroup<Granularity>(
        Page, Panel, Tile, Thumbnail, Item);
}
