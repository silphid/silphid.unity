using System;
using System.Linq;
using Silphid.Extensions;
using UnityEngine;

namespace Silphid.Showzup
{
    [Serializable]
    public class Mapping
    {
        [SerializeField] private ClassTypeReference _source;
        [SerializeField] private VariantSet _variants;

        public Type Source => _source;
        public VariantSet Variants => _variants;

        public Mapping(Type source, VariantSet variants)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            if (variants == null)
                throw new ArgumentNullException(nameof(variants));

            _source = source;
            _variants = variants;
        }

        public virtual bool Matches(string filter) =>
            filter.IsNullOrEmpty() ||
            Source.Name.CaseInsensitiveContains(filter) ||
            Variants.Any(x => x.Name.CaseInsensitiveContains(filter));

        public virtual bool IsValid =>
            _source != null &&
            _variants != null;
    }
}