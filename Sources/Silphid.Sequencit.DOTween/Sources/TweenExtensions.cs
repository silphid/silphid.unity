using System;
using DG.Tweening;
using UniRx;
using Rx = UniRx;

namespace Silphid.Sequencit
{
    public static class TweenExtensions
    {
        public static Tween In(this Tween This, ISequencer sequencer)
        {
            This.Pause();
            sequencer.AddSuspension(d =>
            {
                This.Play();
                This.OnComplete(d.Dispose);
            });
            return This;
        }

        public static IObservable<Unit> ToObservable(this Tween This, bool completeTweenOnDispose = false)
        {
            This.Pause();
            return Observable.Create<Unit>(subscriber =>
            {
                This.Play();
                This.OnComplete(() =>
                {
                    subscriber.OnNext(Unit.Default);
                    subscriber.OnCompleted();
                });
                return Disposable.Create(() => This.Kill(completeTweenOnDispose));
            });
        }

        public static void AddTween(this ISequencer This, Func<Tween> action)
        {
            This.Add(() => action().ToObservable());
        }
    }
}