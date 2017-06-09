using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Silphid.Showzup
{
    public class VariantSet : IEnumerable<IVariant>
    {
        private readonly HashSet<IVariant> _hashSet;
        
        public static readonly VariantSet Empty = new VariantSet();
        
        public VariantSet() :
            this(new HashSet<IVariant>())
        {
        }

        public VariantSet(IEqualityComparer<IVariant> comparer) :
            this(new HashSet<IVariant>(comparer))
        {
        }

        public VariantSet([NotNull] IEnumerable<IVariant> collection) :
            this(new HashSet<IVariant>(collection))
        {
        }

        public VariantSet([NotNull] IEnumerable<IVariant> collection, IEqualityComparer<IVariant> comparer) :
            this(new HashSet<IVariant>(collection, comparer))
        {
        }

        private VariantSet(HashSet<IVariant> hashSet)
        {
            _hashSet = hashSet;
        }

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
    }
}