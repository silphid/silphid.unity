using Silphid.Options;

namespace Silphid.Showzup
{
    public class ViewOptions : OptionsBase, IViewOptions
    {
        public static IViewOptions Empty => null;

        public ViewOptions(IViewOptions innerOptions, object key, object value)
            : base(innerOptions, key, value) {}
    }
}