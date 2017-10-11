namespace Silphid.Showzup
{
    public static class OptionsExtensions
    {
        public static Direction GetDirectionOrDefault(this Options This) => This?.Direction ?? Direction.Default;
        public static PushMode GetPushModeOrDefault(this Options This) => This?.PushMode ?? PushMode.Default;
        public static VariantSet GetVariantsOrDefault(this Options This) => This?.Variants ?? VariantSet.Empty;
        
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
        /// Returns a clone of this object, with extra variants.
        /// </summary>
        public static Options With(this Options This, VariantSet variants)
        {
            if (variants.Count == 0)
                return This;
            
            var clone = Options.Clone(This);
            clone.Variants = This?.Variants.UnionWith(variants) ?? variants;
            return clone;
        }

        /// <summary>
        /// Returns a clone of this object, with extra variants.
        /// </summary>
        public static Options With(this Options This, params IVariant[] variants) =>
            variants.Length == 0
                ? This
                : This.With(new VariantSet(variants));

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