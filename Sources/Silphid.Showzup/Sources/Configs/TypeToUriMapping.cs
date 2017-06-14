using System;
using System.Linq;
using UnityEngine;

namespace Silphid.Showzup
{
    [Serializable]
    public class TypeToUriMapping
    {
        [SerializeField] private ClassTypeReference _source;
        [SerializeField] private string _target;
        [SerializeField] private VariantSet _variants;

        public Type Source => _source;
        public Uri Target => new Uri(_target);
        public VariantSet Variants => _variants;

        public TypeToUriMapping(Type source, Uri target, VariantSet variants)
        {
            _source = source;
            _target = target.ToString();
            _variants = variants;
        }

        public override string ToString()
        {
            var variants = _variants.Any() ? $" ({_variants})" : "";
            return $"{_source} => {_target}{variants}";
        }
    }
}