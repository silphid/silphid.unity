using System;
using UniRx;
using UniRx.Triggers;
using UnityEngine.UI;

namespace Silphid.Extensions
{
    public static class UIExtensions
    {
        public static IDisposable BindTo(this Button This, IObservable<bool> canExecute, Action action) =>
            new CompositeDisposable(
                This.OnPointerClickAsObservable()
                    .Subscribe(_ => action()),
                canExecute.BindToInteractable(This));

        public static IDisposable BindTo(this Toggle This, IObservable<bool> canExecute, Action<bool> action) =>
            new CompositeDisposable(
                This.onValueChanged.AsObservable()
                    .Subscribe(action),
                canExecute.BindToInteractable(This));

        public static IObservable<bool> OnToggleAsObservable(this Toggle toggle) =>
            toggle.onValueChanged.AsObservable();
    }
}