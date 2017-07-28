using Silphid.Showzup;

public class Display : Variant<Display>
{
    public static readonly Display Page = Create();
    public static readonly Display Background = Create();
    public static readonly Display Panel = Create();
    public static readonly Display Tile = Create();
    public static readonly Display Thumbnail = Create();
    public static readonly Display Item = Create();
}
