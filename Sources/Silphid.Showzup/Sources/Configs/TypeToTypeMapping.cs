using System;
using System.Linq;
using Silphid.Extensions;
using UnityEngine;

namespace Silphid.Showzup
{
    [Serializable]
    public class TypeToTypeMapping : Mapping
    {
        [SerializeField] private ClassTypeReference _target;
        [SerializeField] private VariantSet _implicitVariants = VariantSet.Empty;

        public Type Target => _target;
        public VariantSet ImplicitVariants
        {
            get { return _implicitVariants; }
            set { _implicitVariants = value; }
        }

        public TypeToTypeMapping(Type source, Type target, VariantSet variants) : base(source, variants)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));
            
            _target = target;
        }

        public override bool Matches(string filter) =>
            base.Matches(filter) ||
            Target.Name.CaseInsensitiveContains(filter) ||
            _implicitVariants.Any(x => x.Name.CaseInsensitiveContains(filter));

        public override string ToString()
        {
            var variants = Variants != null
                ? Variants.Any() ? $" [{Variants}]" : ""
                : "null";
            
            var implicitVariants = _implicitVariants != null
                ? _implicitVariants.Any() ? $" ({_implicitVariants})" : ""
                : "null";

            return $"{Source?.Name.ToStringOrNullLiteral()} => " +
                   $"{Target?.Name.ToStringOrNullLiteral()}" +
                   $"{variants}{implicitVariants}";
        }

        public override bool IsValid =>
            base.IsValid &&
            _target != null &&
            _implicitVariants != null;
    }
}