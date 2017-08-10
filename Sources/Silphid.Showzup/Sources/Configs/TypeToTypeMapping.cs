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
        [SerializeField] private VariantSet _implicitVariants = VariantSet.Empty;

        public Type Source => _source;
        public Type Target => _target;
        public VariantSet Variants => _variants;
        public VariantSet ImplicitVariants
        {
            get { return _implicitVariants; }
            set { _implicitVariants = value; }
        }

        public TypeToTypeMapping(Type source, Type target, VariantSet variants)
        {
            _source = source;
            _target = target;
            _variants = variants;
        }

        public override string ToString()
        {
            var variants = _variants.Any() ? $" [{_variants}]" : "";
            var implicitVariants = _implicitVariants.Any() ? $" ({_implicitVariants})" : "";
            
            return $"{Source.Name} => {Target.Name}{variants}{implicitVariants}";
        }
    }
}