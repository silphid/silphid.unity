using System;
using System.Collections.Generic;
using Silphid.Extensions;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Silphid.Sequencit
{
    public static class IObservableExtensions
    {
        public static IObservable<T> ThrottleOncePerFrame<T>(this IObservable<T> This)
        {
            var lastFrame = -1;

            return This
                .Where(_ =>
                {
                    if (Time.frameCount != lastFrame)
                    {
                        lastFrame = Time.frameCount;
                        return true;
                    }

                    return false;
                })
                .ObserveOn(Scheduler.MainThreadEndOfFrame);
        }

        public static IDisposable BindTo(this Button This, IObservable<bool> canExecute, Action action) =>
            new CompositeDisposable(
                This.OnClickAsObservable().Subscribe(_ => action()),
                canExecute.BindToInteractable(This));

        public static IObservable<bool> OnToggleAsObservable(this Toggle toggle) =>
            toggle.onValueChanged.AsObservable();

        public static IDisposable BindTo(this Toggle This, IObservable<bool> canExecute, Action<bool> action) =>
            new CompositeDisposable(
                This.OnToggleAsObservable().Subscribe(action),
                canExecute.BindToInteractable(This));

        public static void In(this IObservable<Unit> This, ISequencer sequencer)
        {
            sequencer.Add(This);
        }

        public static Sequence ToSequence(this IEnumerable<IObservable<Unit>> This) =>
            Sequence.Create(This);

        public static Parallel ToParallel(this IEnumerable<IObservable<Unit>> This) =>
            Parallel.Create(This);
    }
}