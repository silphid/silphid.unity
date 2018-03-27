using System;
using Silphid.Loadzup;
using Silphid.Loadzup.Http.Caching;

namespace Silphid.Showzup
{
    public interface IBinder
    {
        IView View { get; }
        ILoader Loader { get; }
        HttpCachePolicy? DefaultImageHttpCachePolicy { get; }
        void Add(IDisposable disposable);
    }
}