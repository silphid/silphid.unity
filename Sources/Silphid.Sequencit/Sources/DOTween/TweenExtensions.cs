using System;
using DG.Tweening;
using UniRx;

namespace Silphid.Sequencit
{
    public static class TweenExtensions
    {
        public static Tween In(this Tween This, ISequencer sequencer)
        {
            This.Pause();
            sequencer.AddLapse(d =>
            {
                This.Play();
                This.OnComplete(d.Dispose);
            });
            return This;
        }

        public static ICompletable ToObservable(this Tween This, bool completeTweenOnDispose = false)
        {
            This.Pause();
            return Completable.Create(subscriber =>
            {
                This.Play();
                This.OnComplete(subscriber.OnCompleted);
                return Disposable.Create(() => This.Kill(completeTweenOnDispose));
            });
        }

        public static void AddTween(this ISequencer This, Func<Tween> action)
        {
            This.Add(() => action().ToObservable());
        }
    }
}