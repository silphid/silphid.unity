using System;
using UniRx;
using IDisposable = System.IDisposable;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Silphid.Extensions
{
    public static class ReactivePropertyExtensions
    {
        public static IDisposable BindTwoWayTo<TSource, TTarget>(this IReactiveProperty<TSource> This, IReactiveProperty<TTarget> target)
        {
            if (This == null || target == null)
                return Disposable.Empty;
            
            bool isUpdating = false;

            return new CompositeDisposable(
                This
                    .Where(_ => !isUpdating)
                    .Subscribe(x =>
                    {
                        isUpdating = true;
                        target.Value = (TTarget) (object) x;
                        isUpdating = false;
                    }),
                target
                    .Where(_ => !isUpdating)
                    .Subscribe(x =>
                    {
                        isUpdating = true;
                        This.Value = (TSource) (object) x;
                        isUpdating = false;
                    }));
        }

        public static IDisposable BindToInteractable(this IObservable<bool> This, Selectable selectable) =>
            This.Subscribe(x => selectable.interactable = x);

        public static IDisposable BindToSlider(this IReactiveProperty<float> This, Slider slider)
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

        public static void Toggle(this IReactiveProperty<bool> This) =>
            This.Value = !This.Value;
    }
}