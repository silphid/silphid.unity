using System;
using System.Linq;
using UnityEngine;

namespace Silphid.Showzup
{
    [Serializable]
    public class TypeToUriMapping : Mapping
    {
        [SerializeField] private string _target;

        public Uri Target => new Uri(_target);

        public TypeToUriMapping(Type source, Uri target, VariantSet variants) : base(source, variants)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            _target = target.ToString();
        }

        public override string ToString()
        {
            var variants = Variants.Any() ? $" [{Variants}]" : "";
            return $"{Source.Name} => {_target}{variants}";
        }
    }
}