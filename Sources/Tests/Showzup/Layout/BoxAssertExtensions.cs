using Silphid.Showzup.Layout;
using Silphid.Tests;
using UnityEngine;

namespace Silphid.Showzup.Test.Layout
{
    public static class BoxAssertExtensions
    {
        public static void Is(this IBox This, Rect expected)
        {
            This.GetMin().Is(expected.min, "min");
            This.GetMax().Is(expected.max, "max");
            This.GetSize().Is(expected.size, "size");
        }
        
        public static void Is(this IBox This, Vector2 min, Vector2 size) =>
            This.Is(new Rect(min, size));
    }
}