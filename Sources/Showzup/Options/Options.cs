using Silphid.Options;

namespace Silphid.Showzup
{
    public class Options : OptionsBase, IOptions
    {
        public static IOptions Empty => null;

        public Options(IOptions innerOptions, object key, object value)
            : base(innerOptions, key, value) {}
    }
}