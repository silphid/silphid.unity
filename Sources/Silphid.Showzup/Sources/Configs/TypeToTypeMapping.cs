using System;
using System.Linq;
using UnityEngine;

namespace Silphid.Showzup
{
    [Serializable]
    public class TypeToTypeMapping
    {
        [SerializeField] private ClassTypeReference _source;
        [SerializeField] private ClassTypeReference _target;
        [SerializeField] private VariantSet _variants;

        public Type Source => _source;
        public Type Target => _target;
        public VariantSet Variants => _variants;

        public TypeToTypeMapping(Type source, Type target, VariantSet variants)
        {
            _source = source;
            _target = target;
            _variants = variants;
        }

        public override string ToString()
        {
            var variants = _variants.Any() ? $" ({_variants})" : "";
            return $"{_source} => {_target}{variants}";
        }
    }
}