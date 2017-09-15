using Silphid.Showzup;

public class Platform : Variant<Platform>
{
    public static readonly Platform iOS = new Platform();
    public static readonly Platform Android = new Platform();
    public static readonly Platform WSA = new Platform();
}