using Silphid.Extensions;

namespace Silphid.DataTypes
{
    public struct FloatRange
    {
        public static FloatRange Empty = new FloatRange(-1, -1);

        public readonly float Start;
        public readonly float End;

        public float Size => End - Start;
        public bool IsEmpty => Start.IsAlmostEqualTo(End);

        public FloatRange(float start, float end)
        {
            Start = start;
            End = end;
        }

        public bool Contains(float value) =>
            !IsEmpty && value >= Start && value <= End;

        public bool IntersectsWith(FloatRange other) =>
            !IsEmpty && !other.IsEmpty && (Start <= other.Start && End >= other.End ||
                                           Start >= other.Start && Start < other.End ||
                                           End > other.Start && End <= other.End);

        public FloatRange IntersectionWith(FloatRange other) =>
            IsEmpty || other.IsEmpty
                ? Empty
                : new FloatRange(Start.Max(other.Start), End.Min(other.End));

        public FloatRange ExpandStartAndEndBy(float value) =>
            IsEmpty
                ? Empty
                : new FloatRange(Start - value, End + value);

        public static FloatRange operator +(FloatRange This, float value) =>
            new FloatRange(This.Start + value, This.End + value);

        public static FloatRange operator -(FloatRange This, float value) =>
            new FloatRange(This.Start - value, This.End - value);
            
        public bool Equals(FloatRange other) =>
            Start.IsAlmostEqualTo(other.Start) && End.IsAlmostEqualTo(other.End);

        public override bool Equals(object obj) =>
            obj is FloatRange && Equals((FloatRange) obj);

        public override int GetHashCode()
        {
            unchecked
            {
                return (Start.GetHashCode() * 397) ^ End.GetHashCode();
            }
        }

        public override string ToString() =>
            $"[{Start}, {End}]";
    }
}