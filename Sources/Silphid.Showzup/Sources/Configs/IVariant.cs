namespace Silphid.Showzup
{
    public interface IVariant
    {
        int Id { get; }
        string Name { get; }
        IVariantGroup Group { get; set; }
    }
}