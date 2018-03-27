using System;
using Silphid.Loadzup;
using Silphid.Loadzup.Http.Caching;
using UniRx;

namespace Silphid.Showzup
{
    public class Binder : IBinder
    {
        public IView View { get; }
        public ILoader Loader { get; }
        public HttpCachePolicy? DefaultImageHttpCachePolicy { get; }

        public Binder(IView view, ILoader loader, HttpCachePolicy? defaultImageHttpCachePolicy)
        {
            View = view;
            Loader = loader;
            DefaultImageHttpCachePolicy = defaultImageHttpCachePolicy;
        }

        public void Add(IDisposable disposable)
        {
            disposable.AddTo(View.GameObject);
        }
    }
}