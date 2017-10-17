using Silphid.Loadzup.Http.Caching;

namespace Silphid.Loadzup
{
    public class HttpCachePolicyLoaderDecorator : OptionsLoaderDecoratorBase
    {
        private readonly HttpCachePolicy? _policy;

        public HttpCachePolicyLoaderDecorator(ILoader loader, HttpCachePolicy? policy) : base(loader)
        {
            _policy = policy;
        }

        protected override void UpdateOptions(Options options)
        {
            if (options.HttpCachePolicy == null && _policy != null)
                options.HttpCachePolicy = _policy;
        }
    }
}