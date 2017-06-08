using System;
using System.Collections.Generic;

namespace Silphid.Showzup
{
    [Serializable]
    public class TypeToUriMapping
    {
        public Type Source { get; }
        public Uri Target { get; }
        public List<IVariant> Variants { get; }

        public TypeToUriMapping(Type source, Uri target, List<IVariant> variants)
        {
            Source = source;
            Target = target;
            Variants = variants;
        }
    }
}