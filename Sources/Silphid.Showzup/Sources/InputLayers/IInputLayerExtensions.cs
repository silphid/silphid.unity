using System;
using UniRx;

namespace Silphid.Showzup.InputLayers
{
    public static class IInputLayerExtensions
    {
        public static IDisposable BindTo(this IObservable<bool> This, IInputLayer inputLayer, string reason)
        {
            var disableDisposable = new SerialDisposable();

            return new CompositeDisposable(
                disableDisposable,
                This
                    .DistinctUntilChanged()
                    .Subscribe(x => disableDisposable.Disposable = x
                        ? null
                        : inputLayer.Disable(reason)));
        }
    }
}