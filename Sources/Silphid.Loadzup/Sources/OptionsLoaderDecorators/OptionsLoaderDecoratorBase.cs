using System;

namespace Silphid.Loadzup
{
    public abstract class OptionsLoaderDecoratorBase : ILoader
    {
        private readonly ILoader _loader;

        protected OptionsLoaderDecoratorBase(ILoader loader)
        {
            _loader = loader;
        }

        protected abstract void UpdateOptions(Options options);

        private Options GetOptions(Options options)
        {
            if (options == null)
                options = new Options();
            
            UpdateOptions(options);
            return options;
        }

        #region ILoader members

        public bool Supports<T>(Uri uri) =>
            _loader.Supports<T>(uri);

        public IObservable<T> Load<T>(Uri uri, Options options = null) =>
            _loader.Load<T>(uri, GetOptions(options));

        #endregion
    }
}