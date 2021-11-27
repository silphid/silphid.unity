using System;

namespace Silphid.Showzup.Resolving
{
    public class Problem : IEquatable<Problem>
    {
        public Problem(TypeModel type, VariantSet variants)
        {
            Type = type;
            Variants = variants;
        }

        public TypeModel Type { get; }
        public VariantSet Variants { get; }

        public override string ToString()
        {
            return $"Problem Type: {Type} Variants: {Variants}";
        }

        public bool Equals(Problem other)
        {
            if (other == null)
                return false;

            return Type == other.Type && Variants.SetEquals(other.Variants);
        }

        public override int GetHashCode()
        {
            if (Type == null)
                return 0;

            var hash = 17;

            hash = hash * 23 + Type.GetHashCode();

            if (Variants != null)
                hash = hash * 23 + Variants.ToString()
                                           .GetHashCode();

            return hash;
        }
    }
}