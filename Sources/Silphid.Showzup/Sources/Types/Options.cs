using System.Collections.Generic;
using System.Linq;
using Silphid.Extensions;

namespace Silphid.Showzup
{
    public class Options
    {
        public Direction Direction { get; set; }
        public PushMode PushMode { get; set; }
        public List<string> Variants { get; set; } = new List<string>();
        public Transition Transition { get; set; }
        public float? TransitionDuration { get; set; }

        public override string ToString() =>
            $"{nameof(Direction)}: {Direction}, {nameof(PushMode)}: {PushMode}, {nameof(Variants)}: {Variants?.ToDelimitedString(";")}, {nameof(Transition)}: {Transition}, {nameof(TransitionDuration)}: {TransitionDuration}";

        public static Options Clone(Options other) =>
            new Options
            {
                Direction = other?.Direction ?? Direction.Default,
                PushMode = other?.PushMode ?? PushMode.Default,
                Variants = other?.Variants.ToList() ?? new List<string>(),
                Transition = other?.Transition,
                TransitionDuration = other?.TransitionDuration
            };

        public static Options CloneWithExtraVariants(Options other, IEnumerable<string> variants)
        {
            var clone = Clone(other);
            clone.Variants.AddRange(variants);
            return clone;
        }
    }
}