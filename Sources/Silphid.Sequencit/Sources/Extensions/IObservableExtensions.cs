using System;
using System.Collections.Generic;
using Silphid.Extensions;
using UniRx;
using Rx = UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace Silphid.Sequencit
{
    public static class IObservableExtensions
    {
        public static Rx.IObservable<T> ThrottleOncePerFrame<T>(this Rx.IObservable<T> This)
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

        public static IDisposable BindTo(this Button This, Rx.IObservable<bool> canExecute, Action action) =>
            new CompositeDisposable(
                This.OnClickAsObservable().Subscribe(_ => action()),
                canExecute.BindToInteractable(This));

        public static Rx.IObservable<bool> OnToggleAsObservable(this Toggle toggle) =>
            toggle.onValueChanged.AsObservable();

        public static IDisposable BindTo(this Toggle This, Rx.IObservable<bool> canExecute, Action<bool> action) =>
            new CompositeDisposable(
                This.OnToggleAsObservable().Subscribe(action),
                canExecute.BindToInteractable(This));

        public static void In(this Rx.IObservable<Unit> This, ISequencer sequencer)
        {
            sequencer.Add(This);
        }

        public static Sequence ToSequence(this IEnumerable<Rx.IObservable<Unit>> This) =>
            Sequence.Create(This);

        public static Parallel ToParallel(this IEnumerable<Rx.IObservable<Unit>> This) =>
            Parallel.Create(This);
    }
}