using System;
using Silphid.Loadzup;
using UniRx;

namespace Silphid.Showzup
{
    public class Binder : IBinder
    {
        public IView View { get; }
        public ILoader Loader { get; }

        public Binder(IView view, ILoader loader)
        {
            View = view;
            Loader = loader;
        }

        public IDisposable Add(IDisposable disposable)
        {
            return disposable.AddTo(View.GameObject);
        }
    }
}