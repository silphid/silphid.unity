using System.Collections;
using System.Collections.Generic;
using Silphid.Extensions;

namespace Silphid.DataTypes
{
    public struct IntRange : IEnumerable<int>
    {
        public static IntRange Empty = new IntRange(int.MaxValue, int.MinValue);

        public readonly int Start;
        public readonly int End;

        public int Size =>
            Start == int.MaxValue || End == int.MinValue
                ? 0
                : (End - Start).AtLeast(0);

        public bool IsEmpty => Size == 0;

        public IntRange(int start, int end)
        {
            Start = start;
            End = end;
        }

        public IntRange Union(int index) =>
            new IntRange(Start.Min(index), End.Max(index + 1));

        public IntRange Union(IntRange range) =>
            new IntRange(Start.Min(range.Start), End.Max(range.End));

        public bool Contains(int index) =>
            index >= Start && index < End;

        public bool IntersectsWith(IntRange other) =>
            !IsEmpty &&
            !other.IsEmpty &&
            (Start <= other.Start && End >= other.End ||
             Start >= other.Start && Start < other.End ||
             End > other.Start && End <= other.End);

        public IntRange IntersectionWith(IntRange range) =>
            new IntRange(Start.Max(range.Start), End.Min(range.End));

        public IntRange ExpandStartAndEndBy(int count) =>
            new IntRange(Start - count, End + count);

        public bool Equals(IntRange other) =>
            Start == other.Start && End == other.End;

        public IEnumerator<int> GetEnumerator()
        {
            for (int i = Start; i < End; i++)
                yield return i;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override bool Equals(object obj) =>
            obj is IntRange && Equals((IntRange) obj);

        public override int GetHashCode()
        {
            unchecked
            {
                return (Start * 397) ^ End;
            }
        }

        public override string ToString() =>
            IsEmpty
                ? "Empty"
                : $"[{Start}, {End - 1}]";
    }
}