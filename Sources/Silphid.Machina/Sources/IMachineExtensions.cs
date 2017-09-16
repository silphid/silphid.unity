using System;
using UniRx;

namespace Silphid.Machina
{
    public static class IMachineExtensions
    {
	    public static void Enter<TState>(this IMachine This) where TState : new() =>
		    This.Enter(new TState());

	    public static IObservable<Transition<TSource, TTarget>> Transitioning<TSource, TTarget>(this IMachine This) =>
		    This.Transitions
			    .Where(x => x.Source is TSource && x.Target is TTarget)
			    .Select(x => new Transition<TSource, TTarget>((TSource) x.Source, (TTarget) x.Target));

	    public static IObservable<Transition<TSource, TTarget>> Transitioning<TSource, TTarget>(this IMachine This, TSource source, TTarget target) =>
		    This.Transitioning<TSource, TTarget>()
			    .Where(x => Equals(x.Source, source) && Equals(x.Target, target));

	    public static IObservable<TTarget> Entering<TTarget>(this IMachine This) =>
		    This.Transitions
			    .Select(x => x.Target)
			    .OfType<object, TTarget>();

	    public static IObservable<TSource> Exiting<TSource>(this IMachine This) =>
		    This.Transitions
			    .Select(x => x.Source)
			    .OfType<object, TSource>();

	    public static IObservable<TTarget> Entering<TTarget>(this IMachine This, TTarget target) =>
		    This.Entering<TTarget>()
	    		.Where(x => Equals(x, target));

	    public static IObservable<TSource> Exiting<TSource>(this IMachine This, TSource source) =>
		    This.Exiting<TSource>()
			    .Where(x => Equals(x, source));

	    public static IObservable<long> EveryUpdate(this IObservable<bool> This) =>
		    Observable.Create<long>(observer =>
		    {
			    var everyUpdateSubscription = new SerialDisposable();

			    return new CompositeDisposable(
				    everyUpdateSubscription,
				    This.Subscribe(isState =>
					    everyUpdateSubscription.Disposable = isState
						    ? Observable.EveryUpdate().Subscribe(observer)
						    : null));
		    });
    }
}