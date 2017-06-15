namespace Silphid.Showzup
{
    public class Options
    {
        public Direction Direction { get; set; }
        public PushMode PushMode { get; set; }
        public VariantSet Variants { get; set; } = VariantSet.Empty;
        public Transition Transition { get; set; }
        public float? TransitionDuration { get; set; }

        public override string ToString() =>
            $"{nameof(Direction)}: {Direction}, {nameof(PushMode)}: {PushMode}, {nameof(Variants)}: {Variants}, {nameof(Transition)}: {Transition}, {nameof(TransitionDuration)}: {TransitionDuration}";

        public static Options Clone(Options other) =>
            new Options
            {
                Direction = other?.Direction ?? Direction.Default,
                PushMode = other?.PushMode ?? PushMode.Default,
                Variants = other?.Variants ?? VariantSet.Empty,
                Transition = other?.Transition,
                TransitionDuration = other?.TransitionDuration
            };

        public static Options CloneWithExtraVariants(Options other, VariantSet extraVariants)
        {
            var clone = Clone(other);
            clone.Variants = other?.Variants.UnionWith(extraVariants) ?? extraVariants;
            return clone;
        }
    }
}