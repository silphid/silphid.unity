using Silphid.Showzup;

public class Platform : Variant<Platform>
{
    public static readonly Platform iOS = Create();
    public static readonly Platform Android = Create();
    public static readonly Platform WSA = Create();
}