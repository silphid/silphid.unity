namespace Silphid.Showzup
{
    public static class OptionsExtensions
    {
        public static Direction GetDirection(this Options This) => This?.Direction ?? Direction.Default;
        public static PushMode GetPushMode(this Options This) => This?.PushMode ?? PushMode.Default;
        public static VariantSet GetVariants(this Options This) => This?.Variants ?? VariantSet.Empty;
        
        /// <summary>
        /// Returns a clone of this object, with an overriden Direction.
        /// </summary>
        public static Options With(this Options This, Direction direction)
        {
            var clone = Options.Clone(This);
            clone.Direction = direction;
            return clone;
        }

        /// <summary>
        /// Returns a clone of this object, with an overriden Transition.
        /// </summary>
        public static Options With(this Options This, ITransition transition)
        {
            var clone = Options.Clone(This);
            clone.Transition = transition;
            return clone;
        }

        /// <summary>
        /// Returns a clone of this object, with an overriden TransitionDuration.
        /// </summary>
        public static Options WithDuration(this Options This, float duration)
        {
            var clone = Options.Clone(This);
            clone.TransitionDuration = duration;
            return clone;
        }
    }
}