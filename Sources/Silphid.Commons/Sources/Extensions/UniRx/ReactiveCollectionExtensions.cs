using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace Silphid.Extensions
{
    public struct CollectionAddRemoveEvent<T> : IEquatable<CollectionAddRemoveEvent<T>>
    {
        public int Index { get; }
        public T Value { get; }
        public bool IsAdded { get; }

        public CollectionAddRemoveEvent(int index, T value, bool isAdded) : this()
        {
            Index = index;
            Value = value;
            IsAdded = isAdded;
        }

        public override string ToString() =>
            $"Index: {Index} Value: {Value} IsAdded: {IsAdded}";

        public override int GetHashCode() =>
            Index.GetHashCode() ^ EqualityComparer<T>.Default.GetHashCode(Value) << 2;

        public bool Equals(CollectionAddRemoveEvent<T> other) =>
            Index.Equals(other.Index) && EqualityComparer<T>.Default.Equals(Value, other.Value);
    }

    public static class ReactiveCollectionExtensions
    {
        public static IObservable<CollectionAddRemoveEvent<T>> ObserveCurrentAddRemove<T>(this ReactiveCollection<T> This) =>
            This.Select((x, i) => new CollectionAddRemoveEvent<T>(i, x, true))
                .ToObservable()
                .Concat(This
                    .ObserveAdd()
                    .Select(x => new CollectionAddRemoveEvent<T>(x.Index, x.Value, true))
                    .Merge(This
                        .ObserveRemove()
                        .Select(x => new CollectionAddRemoveEvent<T>(x.Index, x.Value, false))));

    }
}