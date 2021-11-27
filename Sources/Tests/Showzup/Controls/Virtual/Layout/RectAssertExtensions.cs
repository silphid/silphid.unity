using Silphid.Tests;
using UnityEngine;

namespace Silphid.Showzup.Test.Controls.Virtual.Layout
{
    public static class RectAssertExtensions
    {
        public static void Is(this Rect This, Vector2 min, Vector2 size) =>
            This.Is(new Rect(min, size));

        public static void Is(this Rect This, float minX, float minY, Vector2 size) =>
            This.Is(new Rect(minX, minY, size.x, size.y));

        public static void Is(this Rect This, float minX, float minY, float width, float height) =>
            This.Is(new Rect(minX, minY, width, height));
    }
}