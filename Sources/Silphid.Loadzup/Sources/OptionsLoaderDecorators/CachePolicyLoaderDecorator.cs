using Silphid.Loadzup.Caching;

namespace Silphid.Loadzup
{
    public class CachePolicyLoaderDecorator : OptionsLoaderDecoratorBase
    {
        private readonly CachePolicy? _cachePolicy;

        public CachePolicyLoaderDecorator(ILoader loader, CachePolicy? cachePolicy) : base(loader)
        {
            _cachePolicy = cachePolicy;
        }

        protected override void UpdateOptions(Options options)
        {
            if (options.CachePolicy == null && _cachePolicy != null)
                options.CachePolicy = _cachePolicy;
        }
    }
}