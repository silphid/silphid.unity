using System;
using UniRx;
using IDisposable = System.IDisposable;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Silphid.Extensions
{
    public static class ReactivePropertyExtensions
    {
        public static IDisposable BindTo<TSource, TTarget>(this IObservable<TSource> This, IReactiveProperty<TTarget> target) where TSource : TTarget =>
            This.Subscribe(x => target.Value = x);

        public static IDisposable BindToInteractable(this IObservable<bool> This, Selectable selectable) =>
            This.Subscribe(x => selectable.interactable = x);

        public static IDisposable BindToSlider(this ReactiveProperty<float> This, Slider slider)
        {
            bool isChanging = false;

            // Listen for changes in slider's value
            UnityAction<float> onSliderValueChanged = x =>
            {
                // Prevent updating reactive property if change originated from it
                if (isChanging)
                    return;

                isChanging = true;
                This.Value = x;
                isChanging = false;
            };
            slider.onValueChanged.AddListener(onSliderValueChanged);

            // Listen for changes in reactive property's value
            var disposable = This.Subscribe(x =>
            {
                // Prevent updating slider if change originated from it
                if (isChanging)
                    return;

                isChanging = true;
                slider.value = x;
                isChanging = false;
            });

            // Setup disposal
            return Disposable.Create(() =>
            {
                disposable.Dispose();
                slider.onValueChanged.RemoveListener(onSliderValueChanged);
            });
        }
    }
}