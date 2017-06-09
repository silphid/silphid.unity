namespace Silphid.Showzup
{
    public static class OptionsExtensions
    {
        public static Direction GetDirection(this Options This) => This?.Direction ?? Direction.Default;
        public static PushMode GetPushMode(this Options This) => This?.PushMode ?? PushMode.Default;
        public static VariantSet GetVariants(this Options This) => This?.Variants ?? VariantSet.Empty;
    }
}