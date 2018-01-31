using System;
using Silphid.Extensions;
using UniRx;
using Math = UnityEngine.Mathf;

namespace Silphid.Tweenzup
{
    public static class EasingExtensions
    {
	    /// <summary>
	    /// Constant Pi.
	    /// </summary>
	    private const float Pi = Math.PI; 
	
	    /// <summary>
	    /// Constant Pi / 2.
	    /// </summary>
	    private const float HalfPi = Math.PI / 2f;

	    /// <summary>
	    /// Makes this easer animate forwards then backwards within the same time span
	    /// </summary>
	    public static IEaser PingPong(this IEaser This) =>
		    new Easer(t => This.Ease(
			    t <= 0.5f
				    ? t * 2
				    : 1 - (t - 0.5f) * 2));

	    /// <summary>
	    /// Makes this easer animate backwards within the same time span
	    /// </summary>
	    public static IEaser Reversed(this IEaser This) =>
		    new Easer(t => This.Ease(1 - t));
	    
	    /// <summary>
	    /// Converts this Ease enum value into an IEaser
	    /// </summary>
	    public static IEaser ToEaser(this Ease This) =>
		    Easer.From(This);

	    /// <summary>
	    /// Eases this value using given easer
	    /// </summary>
	    public static float Ease(this float This, IEaser ease) =>
		    ease.Ease(This);

	    /// <summary>
	    /// Eases values of This observable using given ease function
	    /// </summary>
	    public static IObservable<float> Ease(this IObservable<float> This, IEaser easer) =>
		    This.Select(easer.Ease);

	    /// <summary>
	    /// Modeled after the parabola y = x^2
	    /// </summary>
	    public static float EaseInQuadratic(this float This)
        {
            return This * This;
        }

	    /// <summary>
	    /// Modeled after the parabola y = -x^2 + 2x
	    /// </summary>
	    public static float EaseOutQuadratic(this float This)
        {
            return -(This * (This - 2));
        }

	    /// <summary>
	    /// Modeled after the piecewise quadratic
	    /// y = (1/2)((2x)^2)             ; [0, 0.5)
	    /// y = -(1/2)((2x-1)*(2x-3) - 1) ; [0.5, 1]
	    /// </summary>
	    public static float EaseInOutQuadratic(this float This)
        {
            if (This < 0.5f)
                return 2 * This * This;

            return -2 * This * This + 4 * This - 1;
        }

	    /// <summary>
	    /// Modeled after the cubic y = x^3
	    /// </summary>
	    public static float EaseInCubic(this float This)
        {
            return This * This * This;
        }

	    /// <summary>
	    /// Modeled after the cubic y = (x - 1)^3 + 1
	    /// </summary>
	    public static float EaseOutCubic(this float This)
        {
            var f = This - 1;
            return f * f * f + 1;
        }

	    /// <summary>
	    /// Modeled after the piecewise cubic
	    /// y = (1/2)((2x)^3)       ; [0, 0.5)
	    /// y = (1/2)((2x-2)^3 + 2) ; [0.5, 1]
	    /// </summary>
	    public static float EaseInOutCubic(this float This)
        {
            if (This < 0.5f)
                return 4 * This * This * This;

            var f = 2 * This - 2;
            return 0.5f * f * f * f + 1;
        }

	    /// <summary>
	    /// Modeled after the quartic x^4
	    /// </summary>
	    public static float EaseInQuartic(this float This)
        {
            return This * This * This * This;
        }

	    /// <summary>
	    /// Modeled after the quartic y = 1 - (x - 1)^4
	    /// </summary>
	    public static float EaseOutQuartic(this float This)
        {
            var f = This - 1;
            return f * f * f * (1 - This) + 1;
        }

        /// <summary>
        // Modeled after the piecewise quartic
        // y = (1/2)((2x)^4)        ; [0, 0.5)
        // y = -(1/2)((2x-2)^4 - 2) ; [0.5, 1]
        /// </summary>
        public static float EaseInOutQuartic(this float This)
        {
            if (This < 0.5f)
                return 8 * This * This * This * This;

            var f = This - 1;
            return -8 * f * f * f * f + 1;
        }

	    /// <summary>
	    /// Modeled after the quintic y = x^5
	    /// </summary>
	    public static float EaseInQuintic(this float This)
        {
            return This * This * This * This * This;
        }

	    /// <summary>
	    /// Modeled after the quintic y = (x - 1)^5 + 1
	    /// </summary>
	    public static float EaseOutQuintic(this float This)
        {
            var f = This - 1;
            return f * f * f * f * f + 1;
        }

	    /// <summary>
	    /// Modeled after the piecewise quintic
	    /// y = (1/2)((2x)^5)       ; [0, 0.5)
	    /// y = (1/2)((2x-2)^5 + 2) ; [0.5, 1]
	    /// </summary>
	    public static float EaseInOutQuintic(this float This)
        {
            if (This < 0.5f)
                return 16 * This * This * This * This * This;

            var f = 2 * This - 2;
            return 0.5f * f * f * f * f * f + 1;
        }

	    /// <summary>
	    /// Modeled after quarter-cycle of sine wave
	    /// </summary>
	    public static float EaseInSine(this float This)
        {
            return Math.Sin((This - 1) * HalfPi) + 1;
        }

	    /// <summary>
	    /// Modeled after quarter-cycle of sine wave (different phase)
	    /// </summary>
	    public static float EaseOutSine(this float This)
        {
            return Math.Sin(This * HalfPi);
        }

	    /// <summary>
	    /// Modeled after half sine wave
	    /// </summary>
	    public static float EaseInOutSine(this float This)
        {
            return 0.5f * (1 - Math.Cos(This * Pi));
        }

	    /// <summary>
	    /// Modeled after shifted quadrant IV of unit circle
	    /// </summary>
	    public static float EaseInCircular(this float This)
        {
            return 1 - Math.Sqrt(1 - This * This);
        }

	    /// <summary>
	    /// Modeled after shifted quadrant II of unit circle
	    /// </summary>
	    public static float EaseOutCircular(this float This)
        {
            return Math.Sqrt((2 - This) * This);
        }

	    /// <summary>
	    /// Modeled after the piecewise circular function
	    /// y = (1/2)(1 - Math.Sqrt(1 - 4x^2))           ; [0, 0.5)
	    /// y = (1/2)(Math.Sqrt(-(2x - 3)*(2x - 1)) + 1) ; [0.5, 1]
	    /// </summary>
	    public static float EaseInOutCircular(this float This)
        {
            if (This < 0.5f)
                return 0.5f * (1 - Math.Sqrt(1 - 4 * (This * This)));

            return 0.5f * (Math.Sqrt(-(2 * This - 3) * (2 * This - 1)) + 1);
        }

	    /// <summary>
	    /// Modeled after the exponential function y = 2^(10(x - 1))
	    /// </summary>
	    public static float EaseInExponential(this float This)
        {
            return This.IsAlmostZero() ? This : Math.Pow(2, 10 * (This - 1));
        }

	    /// <summary>
	    /// Modeled after the exponential function y = -2^(-10x) + 1
	    /// </summary>
	    public static float EaseOutExponential(this float This)
        {
            return This.IsAlmostEqualTo(1f) ? This : 1 - Math.Pow(2, -10 * This);
        }

	    /// <summary>
	    /// Modeled after the piecewise exponential
	    /// y = (1/2)2^(10(2x - 1))         ; [0,0.5)
	    /// y = -(1/2)*2^(-10(2x - 1))) + 1 ; [0.5,1]
	    /// </summary>
	    public static float EaseInOutExponential(this float This)
        {
            if (This.IsAlmostZero() || This.IsAlmostEqualTo(1f)) return This;

            if (This < 0.5f)
                return 0.5f * Math.Pow(2, 20 * This - 10);

            return -0.5f * Math.Pow(2, -20 * This + 10) + 1;
        }

	    /// <summary>
	    /// Modeled after the damped sine wave y = sin(13pi/2*x)*Math.Pow(2, 10 * (x - 1))
	    /// </summary>
	    public static float EaseInElastic(this float This)
        {
            return Math.Sin(13 * HalfPi * This) * Math.Pow(2, 10 * (This - 1));
        }

	    /// <summary>
	    /// Modeled after the damped sine wave y = sin(-13pi/2*(x + 1))*Math.Pow(2, -10x) + 1
	    /// </summary>
	    public static float EaseOutElastic(this float This)
        {
            return Math.Sin(-13 * HalfPi * (This + 1)) * Math.Pow(2, -10 * This) + 1;
        }

	    /// <summary>
	    /// Modeled after the piecewise exponentially-damped sine wave:
	    /// y = (1/2)*sin(13pi/2*(2*x))*Math.Pow(2, 10 * ((2*x) - 1))      ; [0,0.5)
	    /// y = (1/2)*(sin(-13pi/2*((2x-1)+1))*Math.Pow(2,-10(2*x-1)) + 2) ; [0.5, 1]
	    /// </summary>
	    public static float EaseInOutElastic(this float This)
        {
            if (This < 0.5f)
                return 0.5f * Math.Sin(13 * HalfPi * (2 * This)) * Math.Pow(2, 10 * (2 * This - 1));

            return 0.5f * (Math.Sin(-13 * HalfPi * (2 * This - 1 + 1)) * Math.Pow(2, -10 * (2 * This - 1)) + 2);
        }

	    /// <summary>
	    /// Modeled after the overshooting cubic y = x^3-x*sin(x*pi)
	    /// </summary>
	    public static float EaseInBack(this float This)
        {
            return This * This * This - This * Math.Sin(This * Pi);
        }

	    /// <summary>
	    /// Modeled after overshooting cubic y = 1-((1-x)^3-(1-x)*sin((1-x)*pi))
	    /// </summary>
	    public static float EaseOutBack(this float This)
        {
            var f = 1 - This;
            return 1 - (f * f * f - f * Math.Sin(f * Pi));
        }

	    /// <summary>
	    /// Modeled after the piecewise overshooting cubic function:
	    /// y = (1/2)*((2x)^3-(2x)*sin(2*x*pi))           ; [0, 0.5)
	    /// y = (1/2)*(1-((1-x)^3-(1-x)*sin((1-x)*pi))+1) ; [0.5, 1]
	    /// </summary>
	    public static float EaseInOutBack(this float This)
        {
            if (This < 0.5f)
            {
                var f = 2 * This;
                return 0.5f * (f * f * f - f * Math.Sin(f * Pi));
            }
            else
            {
                var f = 1 - (2 * This - 1);
                return 0.5f * (1 - (f * f * f - f * Math.Sin(f * Pi))) + 0.5f;
            }
        }

        /// <summary>
        /// </summary>
        public static float EaseInBounce(this float This)
        {
            return 1 - EaseOutBounce(1 - This);
        }

        /// <summary>
        /// </summary>
        public static float EaseOutBounce(this float This)
        {
            if (This < 4 / 11.0f)
                return 121 * This * This / 16.0f;

            if (This < 8 / 11.0f)
                return 363 / 40.0f * This * This - 99 / 10.0f * This + 17 / 5.0f;

            if (This < 9 / 10.0f)
                return 4356 / 361.0f * This * This - 35442 / 1805.0f * This + 16061 / 1805.0f;

            return 54 / 5.0f * This * This - 513 / 25.0f * This + 268 / 25.0f;
        }

        /// <summary>
        /// </summary>
        public static float EaseInOutBounce(this float This)
        {
            if (This < 0.5f)
                return 0.5f * EaseInBounce(This * 2);

            return 0.5f * EaseOutBounce(This * 2 - 1) + 0.5f;
        }
    }
}