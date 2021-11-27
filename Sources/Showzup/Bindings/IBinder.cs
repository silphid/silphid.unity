using System;
using Silphid.Loadzup;

namespace Silphid.Showzup
{
    public interface IBinder
    {
        IView View { get; }
        ILoader Loader { get; }
        IDisposable Add(IDisposable disposable);
    }
}