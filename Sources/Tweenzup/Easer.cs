using System;
using Silphid.DataTypes;

namespace Silphid.Tweenzup
{
    public class Easer : TypeSafeEnum<Easer>, IEaser
    {
        #region Statics

        public static readonly IEaser Instant = new Easer("Instant", _ => 1);
        public static readonly IEaser Linear = new Easer("Linear", t => t);
        public static readonly IEaser InQuadratic = new Easer("InQuadratic", EasingExtensions.EaseInQuadratic);
        public static readonly IEaser OutQuadratic = new Easer("OutQuadratic", EasingExtensions.EaseOutQuadratic);
        public static readonly IEaser InOutQuadratic = new Easer("InOutQuadratic", EasingExtensions.EaseInOutQuadratic);
        public static readonly IEaser InCubic = new Easer("InCubic", EasingExtensions.EaseInCubic);
        public static readonly IEaser OutCubic = new Easer("OutCubic", EasingExtensions.EaseOutCubic);
        public static readonly IEaser InOutCubic = new Easer("InOutCubic", EasingExtensions.EaseInOutCubic);
        public static readonly IEaser InQuartic = new Easer("InQuartic", EasingExtensions.EaseInQuartic);
        public static readonly IEaser OutQuartic = new Easer("OutQuartic", EasingExtensions.EaseOutQuartic);
        public static readonly IEaser InOutQuartic = new Easer("InOutQuartic", EasingExtensions.EaseInOutQuartic);
        public static readonly IEaser InQuintic = new Easer("InQuintic", EasingExtensions.EaseInQuintic);
        public static readonly IEaser OutQuintic = new Easer("OutQuintic", EasingExtensions.EaseOutQuintic);
        public static readonly IEaser InOutQuintic = new Easer("InOutQuintic", EasingExtensions.EaseInOutQuintic);
        public static readonly IEaser InSine = new Easer("InSine", EasingExtensions.EaseInSine);
        public static readonly IEaser OutSine = new Easer("OutSine", EasingExtensions.EaseOutSine);
        public static readonly IEaser InOutSine = new Easer("InOutSine", EasingExtensions.EaseInOutSine);
        public static readonly IEaser InCircular = new Easer("InCircular", EasingExtensions.EaseInCircular);
        public static readonly IEaser OutCircular = new Easer("OutCircular", EasingExtensions.EaseOutCircular);
        public static readonly IEaser InOutCircular = new Easer("InOutCircular", EasingExtensions.EaseInOutCircular);
        public static readonly IEaser InExponential = new Easer("InExponential", EasingExtensions.EaseInExponential);
        public static readonly IEaser OutExponential = new Easer("OutExponential", EasingExtensions.EaseOutExponential);

        public static readonly IEaser InOutExponential = new Easer(
            "InOutExponential",
            EasingExtensions.EaseInOutExponential);

        public static readonly IEaser InElastic = new Easer("InElastic", EasingExtensions.EaseInElastic);
        public static readonly IEaser OutElastic = new Easer("OutElastic", EasingExtensions.EaseOutElastic);
        public static readonly IEaser InOutElastic = new Easer("InOutElastic", EasingExtensions.EaseInOutElastic);
        public static readonly IEaser InBack = new Easer("InBack", EasingExtensions.EaseInBack);
        public static readonly IEaser OutBack = new Easer("OutBack", EasingExtensions.EaseOutBack);
        public static readonly IEaser InOutBack = new Easer("InOutBack", EasingExtensions.EaseInOutBack);
        public static readonly IEaser InBounce = new Easer("InBounce", EasingExtensions.EaseInBounce);
        public static readonly IEaser OutBounce = new Easer("OutBounce", EasingExtensions.EaseOutBounce);
        public static readonly IEaser InOutBounc = new Easer("InOutBounc", EasingExtensions.EaseInOutBounce);

        /// <summary>
        /// Converts an Ease enum value into an IEaser
        /// </summary>
        public static IEaser From(Ease ease) =>
            FromValue((int) ease);

        #endregion

        private readonly Func<float, float> _func;

        public Easer(string name, Func<float, float> func)
            : base(name)
        {
            _func = func;
        }

        public float Ease(float t) =>
            _func(t);
    }
}