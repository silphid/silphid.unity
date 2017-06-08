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
    }
}