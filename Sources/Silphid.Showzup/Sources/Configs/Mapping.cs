using System;
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

        public Mapping(ClassTypeReference source, VariantSet variants)
        {
            _source = source;
            _variants = variants;
        }
    }
}