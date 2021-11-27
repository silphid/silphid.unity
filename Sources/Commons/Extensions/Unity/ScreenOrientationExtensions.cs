using JetBrains.Annotations;
using UnityEngine;

namespace Silphid.Extensions
{
    public static class ScreenOrientationExtensions
    {
        /// <summary>
        /// Returns true if the ScreenOrientation is Portrait or PortraitUpsideDown
        /// </summary>
        [Pure]
        public static bool IsPortrait(this ScreenOrientation This) =>
            This == ScreenOrientation.Portrait || This == ScreenOrientation.PortraitUpsideDown;

        /// <summary>
        /// Returns true if the ScreenOrientation is Landscape, LandscapeLeft or LandscapeRight
        /// </summary>
        [Pure]
        public static bool IsLandscape(this ScreenOrientation This) =>
            This == ScreenOrientation.Landscape || This == ScreenOrientation.LandscapeLeft ||
            This == ScreenOrientation.LandscapeRight;
    }
}