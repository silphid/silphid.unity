using System;
using System.Linq;
using Silphid.Extensions;
using UnityEngine;

namespace Silphid.Showzup
{
    [Serializable]
    public class ViewToPrefabMapping : Mapping
    {
        [SerializeField] private string _target;
        [SerializeField] private string _guid;

        public Uri Target => new Uri(_target);
        public string Guid => _guid;

        public ViewToPrefabMapping(Type source, Uri target, string guid, VariantSet variants)
            : base(source, variants)
        {
            if (target == null)
                throw new ArgumentNullException(nameof(target));

            _target = target.ToString();
            _guid = guid;
        }

        public override bool Matches(string filter) =>
            base.Matches(filter) || _target.CaseInsensitiveContains(filter);

        public override string ToString()
        {
            var variants = Variants != null
                               ? Variants.Any()
                                     ? $" [{Variants}]"
                                     : ""
                               : "null";

            return $"{Source?.Name.ToStringOrNullLiteral()} => " + $"{_target.ToStringOrNullLiteral()}" + $"{variants}";
        }

        public override string TargetName => _target;

        public override bool IsValid =>
            base.IsValid && _target != null;
    }
}