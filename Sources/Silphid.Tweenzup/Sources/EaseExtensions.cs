using System;

namespace Silphid.Tweenzup
{
    public static class EaseExtensions
    {
        private static readonly Func<float, float>[] _funcs = 
        {
            t => 1,
            t => t,
            FloatEaseExtensions.EaseInQuadratic,
            FloatEaseExtensions.EaseOutQuadratic,
            FloatEaseExtensions.EaseInOutQuadratic,
            FloatEaseExtensions.EaseInCubic,
            FloatEaseExtensions.EaseOutCubic,
            FloatEaseExtensions.EaseInOutCubic,
            FloatEaseExtensions.EaseInQuartic,
            FloatEaseExtensions.EaseOutQuartic,
            FloatEaseExtensions.EaseInOutQuartic,
            FloatEaseExtensions.EaseInQuintic,
            FloatEaseExtensions.EaseOutQuintic,
            FloatEaseExtensions.EaseInOutQuintic,
            FloatEaseExtensions.EaseInSine,
            FloatEaseExtensions.EaseOutSine,
            FloatEaseExtensions.EaseInOutSine,
            FloatEaseExtensions.EaseInCircular,
            FloatEaseExtensions.EaseOutCircular,
            FloatEaseExtensions.EaseInOutCircular,
            FloatEaseExtensions.EaseInExponential,
            FloatEaseExtensions.EaseOutExponential,
            FloatEaseExtensions.EaseInOutExponential,
            FloatEaseExtensions.EaseInElastic,
            FloatEaseExtensions.EaseOutElastic,
            FloatEaseExtensions.EaseInOutElastic,
            FloatEaseExtensions.EaseInBack,
            FloatEaseExtensions.EaseOutBack,
            FloatEaseExtensions.EaseInOutBack,
            FloatEaseExtensions.EaseInBounce,
            FloatEaseExtensions.EaseOutBounce,
            FloatEaseExtensions.EaseInOutBounce
        };

        public static Func<float, float> GetFunc(this Ease This) =>
            _funcs[(int) This];

        public static float Eval(this Ease This, float t) =>
            This.GetFunc()(t);
    }
}