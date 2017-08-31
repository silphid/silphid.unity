using Silphid.Extensions;

namespace Silphid.Showzup.ListLayouts
{
    public struct IndexRange
    {
        public static IndexRange Empty = new IndexRange(-1, -1);
        
        public readonly int Start;
        public readonly int End;

        public int Size => End - Start;
        public bool IsEmpty => Size == 0;

        public IndexRange(int start, int end)
        {
            Start = start;
            End = end;
        }

        public bool Contains(int index) =>
            index >= Start && index < End;

        public IndexRange IntersectionWith(IndexRange range) =>
            new IndexRange(Start.Max(range.Start), End.Min(range.End));

        public bool Equals(IndexRange other)
        {
            return Start == other.Start && End == other.End;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is IndexRange && Equals((IndexRange) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Start * 397) ^ End;
            }
        }
    }
}