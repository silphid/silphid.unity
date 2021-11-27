using System;

namespace Silphid.Loadzup
{
    public class OptionsDecoratorLoader : ILoader
    {
        private readonly ILoader _loader;
        private readonly Func<IOptions, IOptions> _selector;

        public OptionsDecoratorLoader(ILoader loader, Func<IOptions, IOptions> selector)
        {
            _loader = loader;
            _selector = selector;
        }

        public bool Supports<T>(Uri uri) =>
            _loader.Supports<T>(uri);

        public IObservable<T> Load<T>(Uri uri, IOptions options = null) =>
            _loader.Load<T>(uri, _selector(options));
    }
}