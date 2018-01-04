using System;
using Silphid.Extensions.DataTypes;

namespace Silphid.Tweenzup
{
    public class Easer : ObjectEnum<Easer>, IEaser
    {
        #region Statics

        public static readonly IEaser Instant = new Easer(_ => 1);
        public static readonly IEaser Linear = new Easer(t => t);
        public static readonly IEaser InQuadratic = new Easer(EasingExtensions.EaseInQuadratic);
        public static readonly IEaser OutQuadratic = new Easer(EasingExtensions.EaseOutQuadratic);
        public static readonly IEaser InOutQuadratic = new Easer(EasingExtensions.EaseInOutQuadratic);
        public static readonly IEaser InCubic = new Easer(EasingExtensions.EaseInCubic);
        public static readonly IEaser OutCubic = new Easer(EasingExtensions.EaseOutCubic);
        public static readonly IEaser InOutCubic = new Easer(EasingExtensions.EaseInOutCubic);
        public static readonly IEaser InQuartic = new Easer(EasingExtensions.EaseInQuartic);
        public static readonly IEaser OutQuartic = new Easer(EasingExtensions.EaseOutQuartic);
        public static readonly IEaser InOutQuartic = new Easer(EasingExtensions.EaseInOutQuartic);
        public static readonly IEaser InQuintic = new Easer(EasingExtensions.EaseInQuintic);
        public static readonly IEaser OutQuintic = new Easer(EasingExtensions.EaseOutQuintic);
        public static readonly IEaser InOutQuintic = new Easer(EasingExtensions.EaseInOutQuintic);
        public static readonly IEaser InSine = new Easer(EasingExtensions.EaseInSine);
        public static readonly IEaser OutSine = new Easer(EasingExtensions.EaseOutSine);
        public static readonly IEaser InOutSine = new Easer(EasingExtensions.EaseInOutSine);
        public static readonly IEaser InCircular = new Easer(EasingExtensions.EaseInCircular);
        public static readonly IEaser OutCircular = new Easer(EasingExtensions.EaseOutCircular);
        public static readonly IEaser InOutCircular = new Easer(EasingExtensions.EaseInOutCircular);
        public static readonly IEaser InExponential = new Easer(EasingExtensions.EaseInExponential);
        public static readonly IEaser OutExponential = new Easer(EasingExtensions.EaseOutExponential);
        public static readonly IEaser InOutExponential = new Easer(EasingExtensions.EaseInOutExponential);
        public static readonly IEaser InElastic = new Easer(EasingExtensions.EaseInElastic);
        public static readonly IEaser OutElastic = new Easer(EasingExtensions.EaseOutElastic);
        public static readonly IEaser InOutElastic = new Easer(EasingExtensions.EaseInOutElastic);
        public static readonly IEaser InBack = new Easer(EasingExtensions.EaseInBack);
        public static readonly IEaser OutBack = new Easer(EasingExtensions.EaseOutBack);
        public static readonly IEaser InOutBack = new Easer(EasingExtensions.EaseInOutBack);
        public static readonly IEaser InBounce = new Easer(EasingExtensions.EaseInBounce);
        public static readonly IEaser OutBounce = new Easer(EasingExtensions.EaseOutBounce);
        public static readonly IEaser InOutBounc = new Easer(EasingExtensions.EaseInOutBounce);
        
        /// <summary>
        /// Converts an Ease enum value into an IEaser
        /// </summary>
        public static IEaser From(Ease ease) =>
            FromId((int) ease);
        
        #endregion
        
        private readonly Func<float, float> _func;
        
        public Easer(Func<float, float> func)
        {
            _func = func;
        }

        public float Eval(float t) =>
            _func(t);
    }
}