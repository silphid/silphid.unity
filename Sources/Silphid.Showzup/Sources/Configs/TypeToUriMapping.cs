using System;

namespace Silphid.Showzup
{
    [Serializable]
    public class TypeToUriMapping
    {
        public Type Source { get; }
        public Uri Target { get; }
        public VariantSet Variants { get; }

        public TypeToUriMapping(Type source, Uri target, VariantSet variants)
        {
            Source = source;
            Target = target;
            Variants = variants;
        }
    }
}