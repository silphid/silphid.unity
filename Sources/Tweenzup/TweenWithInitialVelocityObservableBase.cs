using System;
using UniRx;
using UnityEngine;

namespace Silphid.Tweenzup
{
    public abstract class TweenWithInitialVelocityObservableBase<T> : IObservable<T>
    {
        private readonly Func<T> _sourceSelector;
        private readonly Func<T> _velocitySelector;
        private readonly T _target;
        private readonly float _duration;
        private readonly IEaser _easer;

        protected TweenWithInitialVelocityObservableBase(Func<T> sourceSelector,
                                                         Func<T> velocitySelector,
                                                         T target,
                                                         float duration,
                                                         IEaser easer)
        {
            _sourceSelector = sourceSelector;
            _velocitySelector = velocitySelector;
            _target = target;
            _duration = duration;
            _easer = easer ?? Easer.Linear;
        }

        protected abstract T Lerp(float ratio, T source, T target);
        protected abstract T Project(T source, T velocity, float duration);

        public IDisposable Subscribe(IObserver<T> observer)
        {
            var t = 0f;

            var source = _sourceSelector();
            var velocity = _velocitySelector();
            var oldProjectedTarget = Project(source, velocity, _duration);

            var disposable = new SingleAssignmentDisposable();
            disposable.Disposable = Observable.EveryUpdate()
                                              .Subscribe(
                                                   _ =>
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
                                                           var oldProjectedValue = Lerp(t, source, oldProjectedTarget);
                                                           var desiredValue = Lerp(t.Ease(_easer), source, _target);
                                                           var actualValue = Lerp(t, oldProjectedValue, desiredValue);

                                                           observer.OnNext(actualValue);

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