using Silphid.Showzup;

public class Form : Variant<Form>
{
    public static readonly Form Mobile = new Form();
    public static readonly Form Tablet = new Form();
    public static readonly Form TV = new Form();
        
    public new static readonly VariantGroup<Form> Group = new VariantGroup<Form>(
        Mobile, Tablet, TV);
}
