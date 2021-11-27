using JetBrains.Annotations;
using UnityEngine;

namespace Silphid.Extensions
{
    public static class DeviceOrientationExtensions
    {
        /// <summary>
        /// Returns true if the DeviceOrientation is Portrait or PortraitUpsideDown
        /// </summary>
        [Pure]
        public static bool IsPortrait(this DeviceOrientation This) =>
            This == DeviceOrientation.Portrait || This == DeviceOrientation.PortraitUpsideDown;

        /// <summary>
        /// Returns true if the DeviceOrientation is LandscapeLeft or LandscapeRight
        /// </summary>
        [Pure]
        public static bool IsLandscape(this DeviceOrientation This) =>
            This == DeviceOrientation.LandscapeLeft || This == DeviceOrientation.LandscapeRight;
    }
}