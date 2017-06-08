using Silphid.Extensions.DataTypes;

namespace Silphid.Showzup
{
    public class Variant<T> : ObjectEnum<T>, IVariant where T : Variant<T>
    {
        public IVariantGroup Group { get; set; }

        public override string ToString() =>
            $"{Group.Name}:{Name}";
    }
}