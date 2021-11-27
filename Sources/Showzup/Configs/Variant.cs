using Silphid.DataTypes;

namespace Silphid.Showzup
{
    public abstract class Variant<T> : TypeSafeEnum<T>, IVariant where T : Variant<T>
    {
        IVariantGroup IVariant.Group { get; set; }

        private static readonly VariantGroup<T> _group = new VariantGroup<T>();
        public static IVariantGroup Group => _group;

        public IVariant Fallback { get; }

        protected Variant(string name, Variant<T> fallback = null)
            : base(name)
        {
            _group.Add((T) this);
            ((IVariant) this).Group = _group;
            Fallback = fallback;
        }
    }
}