using System;
using DG.Tweening;
using UniRx;
using Rx = UniRx;

namespace Silphid.Sequencit
{
    public static class TweenExtensions
    {
        public static Tween In(this Tween This, ISequenceable sequenceable)
        {
            This.Pause();
            sequenceable.AddSuspension(d =>
            {
                This.Play();
                This.OnComplete(d.Dispose);
            });
            return This;
        }

        public static Rx.IObservable<Unit> ToObservable(this Tween This, bool completeTweenOnDispose = false)
        {
            return Observable.Create<Unit>(subscriber =>
            {
                This.OnComplete(() =>
                {
                    subscriber.OnNext(Unit.Default);
                    subscriber.OnCompleted();
                });
                return Disposable.Create(() => This.Kill(completeTweenOnDispose));
            });
        }

        public static void AddTween(this ISequenceable This, Func<Tween> action)
        {
            This.Add(() => action().ToObservable());
        }
    }
}