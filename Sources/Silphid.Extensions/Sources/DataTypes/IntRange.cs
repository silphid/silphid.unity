using Silphid.Extensions;

namespace Silphid.DataTypes
{
    public struct IntRange
    {
        public static IntRange Empty = new IntRange(-1, -1);
        
        public readonly int Start;
        public readonly int End;

        public int Size => End - Start;
        public bool IsEmpty => Size == 0;

        public IntRange(int start, int end)
        {
            Start = start;
            End = end;
        }

        public bool Contains(int index) =>
            index >= Start && index < End;

        public IntRange IntersectionWith(IntRange range) =>
            new IntRange(Start.Max(range.Start), End.Min(range.End));

        public IntRange ExpandStartAndEndBy(int count) =>
            new IntRange(Start - count, End + count);

        public bool Equals(IntRange other) =>
            Start == other.Start && End == other.End;

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
            $"[{Start}, {End-1}]";
    }
}