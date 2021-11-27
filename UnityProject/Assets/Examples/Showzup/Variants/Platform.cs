using Silphid.Showzup;

namespace App
{
    public class PlatformType : Variant<PlatformType>
    {
        public static readonly PlatformType Android = Add(nameof(Android));
        public static readonly PlatformType IOS = Add(nameof(IOS));

        protected static PlatformType Add(string name) =>
            Add(new PlatformType(name));

        protected PlatformType(string name) : base(name)
        {
        }
    }
}