using JetBrains.Annotations;
using UnityEngine;

namespace Silphid.Extensions.DataTypes
{
    public struct HSVColor
    {
        public float H;
        public float S;
        public float V;

        public Color ToColor()
        {
            return Color.HSVToRGB(H, S, V);
        }

        [Pure]
        public static HSVColor FromColor(Color rgb)
        {
            float h, s, v;
            Color.RGBToHSV(rgb, out h, out s, out v);
            return new HSVColor(h, s, v);
        }

        public HSVColor(float h, float s, float v)
        {
            H = h;
            S = s;
            V = v;
        }
    }
}
