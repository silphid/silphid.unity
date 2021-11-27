using System;

#pragma warning disable 0162

namespace Silphid.Loadzup.Bundles
{
    public class PlatformProvider : IPlatformProvider
    {
        public string GetPlatformName()
        {
#if UNITY_ANDROID
            return "Android";
#elif UNITY_IOS
            return "iOS";
#elif UNITY_TVOS
            return "tvOS";
#endif
            throw new InvalidOperationException("Platform not supported");
        }
    }
}