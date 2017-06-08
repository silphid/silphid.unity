namespace Silphid.Showzup
{
    public interface IVariantGroup
    {
        string Name { get; }
        VariantSet Variants { get; }
    }
}