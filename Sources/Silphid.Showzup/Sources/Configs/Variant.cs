using Silphid.Extensions.DataTypes;

namespace Silphid.Showzup
{
    public abstract class Variant<T> : ObjectEnum<T>, IVariant where T : Variant<T>, new()
    {
        IVariantGroup IVariant.Group { get; set; }

        private static readonly VariantGroup<T> _group = new VariantGroup<T>();
        public static IVariantGroup Group => _group;
        
        public static T Create()
        {
            var variant = new T();
            _group.Add(variant);
            return variant;
        }

        public override string ToString() =>
            $"{((IVariant)this).Group.Name}.{Name}";
    }
}