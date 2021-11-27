using System;
using Silphid.Extensions;
using UniRx;

namespace Silphid.Tweenzup
{
    public abstract class TweenToShiftingTargetCompletableBase<T> : ICompletable
    {
        private readonly IReactiveProperty<T> _property;
        private readonly IObservable<T> _target;
        private readonly float _duration;
        private readonly IEaser _easer;

        protected TweenToShiftingTargetCompletableBase(IReactiveProperty<T> property,
                                                       IObservable<T> target,
                                                       float duration,
                                                       IEaser easer = null)
        {
            _property = property;
            _target = target;
            _duration = duration;
            _easer = easer;
        }

        protected abstract IObservable<T> GetVelocity(IReactiveProperty<T> property);

        protected abstract ICompletable Tween(IReactiveProperty<T> property,
                                              T target,
                                              float duration,
                                              T velocity,
                                              IEaser easer);

        public IDisposable Subscribe(ICompletableObserver observer)
        {
            var velocity = GetVelocity(_property)
               .ToReactiveProperty();

            var outerDisposable = new SingleAssignmentDisposable();
            var currentTween = new SerialDisposable();
            var observerCompletion = new RefCountDisposable(Disposable.Create(observer.OnCompleted));

            outerDisposable.Disposable = new CompositeDisposable(
                _target.Subscribe(
                    x =>
                    {
                        // Prevent observer from completing while there is a tween in progress
                        var tweenCompletion = observerCompletion.GetDisposable();

                        // Dispose any previous tween in progress, if any
                        currentTween.Disposable = new CompositeDisposable(
                            Tween(_property, x, _duration, velocity.Value, _easer)
                               .SubscribeAndForget(tweenCompletion.Dispose),
                            tweenCompletion);
                    },
                    () => observerCompletion.Dispose()),
                currentTween);

            return outerDisposable;
        }
    }
}