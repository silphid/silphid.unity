using System;
using UniRx;
using UnityEngine;

namespace Silphid.Tweenzup
{
    public class TweenObservable : IObservable<float>
    {
        private readonly float _duration;
        private readonly Func<float, float> _ease;

        public TweenObservable(float duration, Func<float, float> ease = null)
        {
            _duration = duration;
            _ease = ease;
        }

        public IDisposable Subscribe(IObserver<float> observer)
        {
            var t = 0f;
            
            var disposable = new SingleAssignmentDisposable();
            disposable.Disposable = Observable.EveryUpdate().Subscribe(_ =>
            {
                t += Time.deltaTime / _duration;
                bool isCompleted = false;
                if (t >= 0.99999f)
                {
                    t = 1f;
                    isCompleted = true;
                }

                try
                {
                    observer.OnNext(_ease?.Invoke(t) ?? t);
                    if (isCompleted)
                        observer.OnCompleted();

                }
                finally
                {
                    if (isCompleted)
                        disposable.Dispose();
                }
            });

            return disposable;
        }
    }
}