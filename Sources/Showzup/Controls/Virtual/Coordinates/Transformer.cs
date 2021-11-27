using Silphid.Showzup.Virtual.Layout;

namespace Silphid.Showzup.Virtual.Coordinates
{
    public static class Transformer
    {
        public static readonly ITransformer UnityToVerticalLayout =
            new FlippedYTransformer(new IdentityTransformer());

        public static readonly ITransformer UnityToHorizontalLayout =
            new FlippedXYTransformer(UnityToVerticalLayout);

        public static ITransformer GetUnityToLayout(Orientation orientation) =>
            orientation == Orientation.Vertical
                ? UnityToVerticalLayout
                : UnityToHorizontalLayout;
    }
}