using JetBrains.Annotations;
using UnityEngine;

namespace Silphid.Extensions
{
    public static class ColorExtensions
    {
        public static Color FromRGB(int r, int g, int b)
        {
            return new Color(r / 255f, g / 255f, b / 255f);
        }

        public static Color FromRGBA(int r, int g, int b, int a)
        {
            return new Color(r / 255f, g / 255f, b / 255f, a / 255f);
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

        [Pure]
        public static bool IsAlmostEqualTo(this Color This, Color other) =>
            This.r.IsAlmostEqualTo(other.r) && This.g.IsAlmostEqualTo(other.g) && This.b.IsAlmostEqualTo(other.b) &&
            This.a.IsAlmostEqualTo(other.a);

        /// <summary>
        /// Uses This value as ratio to interpolate between source and target.
        /// </summary>
        [Pure]
        public static Color Lerp(this float This, Color source, Color target)
        {
            return new Color(
                source.r + (target.r - source.r) * This,
                source.g + (target.g - source.g) * This,
                source.b + (target.b - source.b) * This,
                source.a + (target.a - source.a) * This);
        }
    }
}