using Silphid.Options;

namespace Silphid.Showzup.Flows
{
    public class FlowOptions : OptionsBase, IFlowOptions
    {
        public static IFlowOptions Empty => null;

        public FlowOptions(IFlowOptions innerOptions, object key, object value)
            : base(innerOptions, key, value) {}
    }
}