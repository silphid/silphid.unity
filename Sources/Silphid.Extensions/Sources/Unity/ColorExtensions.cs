using JetBrains.Annotations;
using UnityEngine;

namespace Silphid.Extensions
{
    public static class ColorExtensions
    {
        public static Color FromRGB(int r, int g, int b)
        {
            return new Color(r/255f, g/255f, b/255f);
        }

        public static Color FromRGBA(int r, int g, int b, int a)
        {
            return new Color(r / 255f, g / 255f, b / 255f, a/255f);
        }

        public static Color WithAlpha(this Color color, float alpha)
        {
            return new Color(color.r, color.g, color.b, alpha);
        }

        public static Color ToTransparent(this Color color)
        {
            return color.WithAlpha(0);
        }

        public static Color ToOpaque(this Color color)
        {
            return color.WithAlpha(1);
        }

        /// <summary>
        /// Returns interpolated color at given ratio, between This and target colors.
        /// </summary>
        [Pure]
        public static Color InterpolateTo(this Color This, Color target, float ratio)
        {
            return new Color(
                This.r + ((target.r - This.r) * ratio),
                This.g + ((target.g - This.g) * ratio),
                This.b + ((target.b - This.b) * ratio),
                This.a + ((target.a - This.a) * ratio));
        }
    }
}