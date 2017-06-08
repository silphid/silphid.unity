using Silphid.Showzup;

public class Platform : Variant<Platform>
{
    public static readonly Platform iOS = new Platform();
    public static readonly Platform Android = new Platform();
    public static readonly Platform WSA = new Platform();
        
    public new static readonly VariantGroup<Platform> Group = new VariantGroup<Platform>(
        iOS, Android, WSA);
}