using System;

namespace Silphid.Showzup
{
    public class TypeToTypeMapping
    {
        public Type Source { get; }
        public Type Target { get; }
        public VariantSet Variants { get; }

        public TypeToTypeMapping(Type source, Type target, VariantSet variants)
        {
            Source = source;
            Target = target;
            Variants = variants;
        }

        public override string ToString()
        {
            return $"{nameof(Source)}: {Source}, {nameof(Target)}: {Target}, {nameof(Variants)}: {Variants}";
        }
    }
}