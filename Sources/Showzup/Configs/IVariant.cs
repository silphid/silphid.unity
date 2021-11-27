namespace Silphid.Showzup
{
    public interface IVariant
    {
        int Value { get; }
        string Name { get; }
        IVariantGroup Group { get; set; }
        IVariant Fallback { get; }
    }
}