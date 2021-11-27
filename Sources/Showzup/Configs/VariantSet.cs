using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Silphid.Extensions;
using UnityEngine;

namespace Silphid.Showzup
{
    [Serializable]
    public class VariantSet : IEnumerable<IVariant>, ISerializationCallbackReceiver
    {
        [NonSerialized] private readonly HashSet<IVariant> _hashSet;
        [SerializeField] private List<SerializableVariant> _serializableVariants;

        public static readonly VariantSet Empty = new VariantSet();

        public VariantSet()
            : this(new HashSet<IVariant>()) {}

        public VariantSet(IEqualityComparer<IVariant> comparer)
            : this(new HashSet<IVariant>(comparer)) {}

        public VariantSet([NotNull] params IVariant[] collection)
            : this(new HashSet<IVariant>(collection.WhereNotNull())) {}

        public VariantSet([NotNull] IEnumerable<IVariant> collection)
            : this(new HashSet<IVariant>(collection.WhereNotNull())) {}

        public VariantSet([NotNull] IEnumerable<IVariant> collection, IEqualityComparer<IVariant> comparer)
            : this(new HashSet<IVariant>(collection.WhereNotNull(), comparer)) {}

        private VariantSet(HashSet<IVariant> hashSet)
        {
            _hashSet = hashSet;
        }

        public int Count => _hashSet.Count;

        [Pure]
        public VariantSet UnionWith(IEnumerable<IVariant> other)
        {
            var hashSet = new HashSet<IVariant>(this);
            hashSet.UnionWith(other);
            return new VariantSet(hashSet);
        }

        [Pure]
        public VariantSet IntersectionWith(IEnumerable<IVariant> other)
        {
            var hashSet = new HashSet<IVariant>(this);
            hashSet.IntersectWith(other);
            return new VariantSet(hashSet);
        }

        [Pure]
        public VariantSet ExceptWith(IEnumerable<IVariant> other)
        {
            var hashSet = new HashSet<IVariant>(this);
            hashSet.ExceptWith(other);
            return new VariantSet(hashSet);
        }

        [Pure]
        public VariantSet SymmetricExceptWith(IEnumerable<IVariant> other)
        {
            var hashSet = new HashSet<IVariant>(this);
            hashSet.SymmetricExceptWith(other);
            return new VariantSet(hashSet);
        }

        public IEnumerator<IVariant> GetEnumerator() =>
            _hashSet.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            _hashSet.GetEnumerator();

        public bool IsSubsetOf(IEnumerable<IVariant> other) =>
            _hashSet.IsSubsetOf(other);

        public bool IsSupersetOf(IEnumerable<IVariant> other) =>
            _hashSet.IsSupersetOf(other);

        public bool IsProperSupersetOf(IEnumerable<IVariant> other) =>
            _hashSet.IsProperSupersetOf(other);

        public bool IsProperSubsetOf(IEnumerable<IVariant> other) =>
            _hashSet.IsProperSubsetOf(other);

        public bool Overlaps(IEnumerable<IVariant> other) =>
            _hashSet.Overlaps(other);

        public bool SetEquals(IEnumerable<IVariant> other) =>
            _hashSet.SetEquals(other);

        public override string ToString() =>
            this.JoinAsString(", ");

        #region ISerializationCallbackReceiver members

        public void OnBeforeSerialize()
        {
            if (_serializableVariants == null)
                _serializableVariants = this.Select(x => new SerializableVariant(x))
                                            .ToList();
        }

        public void OnAfterDeserialize()
        {
            _hashSet.Clear();
            _serializableVariants.SelectNotNull(x => x.Variant)
                                 .ForEach(x => _hashSet.Add(x));
        }

        #endregion
    }
}