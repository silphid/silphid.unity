using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

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

        public static void In<T>(this IObservable<T> This, ISequencer sequencer)
        {
            sequencer.Add(This);
        }

        public static Sequence ToSequence(this IEnumerable<IObservable<Unit>> This) =>
            Sequence.Create(This);

        public static Parallel ToParallel(this IEnumerable<IObservable<Unit>> This) =>
            Parallel.Create(This);
    }
}