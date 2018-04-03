using Silphid.DataTypes;

namespace Silphid.Showzup
{
    public abstract class Variant<T> : ObjectEnum<T>, IVariant where T : Variant<T>
    {
        IVariantGroup IVariant.Group { get; set; }

        private static readonly VariantGroup<T> _group = new VariantGroup<T>();
        public static IVariantGroup Group => _group;

        protected Variant()
        {
            _group.Add((T)this);
            ((IVariant)this).Group = _group;
        }

        public override string ToString() => Name;
    }
}