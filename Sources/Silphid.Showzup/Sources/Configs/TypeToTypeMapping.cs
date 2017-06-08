using System;
using System.Collections.Generic;

namespace Silphid.Showzup
{
    public class TypeToTypeMapping
    {
        public Type Source { get; }
        public Type Target { get; }
        public List<IVariant> Variants { get; }

        public TypeToTypeMapping(Type source, Type target, IEnumerable<IVariant> variants)
        {
            Source = source;
            Target = target;
            Variants = new List<IVariant>(variants);
        }
    }
}