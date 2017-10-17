using Silphid.Loadzup.Caching;

namespace Silphid.Loadzup
{
    public class MemoryCachePolicyLoaderDecorator : OptionsLoaderDecoratorBase
    {
        private readonly MemoryCachePolicy? _policy;

        public MemoryCachePolicyLoaderDecorator(ILoader loader, MemoryCachePolicy? policy) : base(loader)
        {
            _policy = policy;
        }

        protected override void UpdateOptions(Options options)
        {
            if (options.MemoryCachePolicy == null && _policy != null)
                options.MemoryCachePolicy = _policy;
        }
    }
}