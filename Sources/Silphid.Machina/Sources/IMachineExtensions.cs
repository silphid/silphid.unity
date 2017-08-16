using System;
using Silphid.Extensions;
using UniRx;

namespace Silphid.Machina
{
    public static class IMachineExtensions
    {
	    public static IObservable<Change<TState>> Changes<TState>(this IObservable<TState> This) =>
		    This.PairWithPrevious().Select(x => new Change<TState>(x.Item1, x.Item2));

	    public static IObservable<Change<TState>> Changes<TState>(this IMachine<TState> This) =>
		    This.State.Changes();

        public static bool Is(this IState This, IState state)
		{
			while (This != null)
			{
				if (This == state)
					return true;

			    This = This.BaseState;
			}

			return false;
		}

	    public static bool IsStateEquivalent(object state, object otherState) =>
		    state is IState && otherState is IState
			    ? ((IState) state).Is((IState) otherState)
			    : state.Equals(otherState);

	    public static IObservable<Change<TState>> Exiting<TState>(this IObservable<Change<TState>> This, TState state)
        {
			return This.Where(x => IsStateEquivalent(x.Source, state));
        }

		public static IObservable<Change<TState>> Entering<TState>(this IObservable<Change<TState>> This, TState state)
        {
			return This.Where(x => IsStateEquivalent(x.Destination, state));
        }

		public static IObservable<Change<TState>> Changing<TState>(this IObservable<Change<TState>> This, TState source, TState destination)
        {
			return This.Exiting(source).Entering(destination);
        }

		public static IObservable<Change<TState>> Exiting<TState>(this IMachine<TState> This, TState state)
        {
            return This.Changes().Exiting(state);
        }

		public static IObservable<Change<TState>> Entering<TState>(this IMachine<TState> This, TState state)
        {
            return This.Changes().Entering(state);
        }

		public static IObservable<Change<TState>> Changing<TState>(this IMachine<TState> This, TState source, TState destination)
        {
			return This.Changes().Changing(source, destination);
        }

		public static IObservable<bool> Is<TState>(this IMachine<TState> This, TState state)
        {
            return This.State.Select(x => IsStateEquivalent(x, state)).DistinctUntilChanged();
        }

		public static IObservable<long> EveryUpdate(this IObservable<bool> This)
		{
			return Observable.Create<long>(observer =>
			{
				var everyUpdateSubscription = Disposable.Empty;
				var isStateSubscription = This.Subscribe(isState =>
				{
					everyUpdateSubscription.Dispose();

					if (isState)
						everyUpdateSubscription = Observable.EveryUpdate().Subscribe(observer);
				});

				return Disposable.Create(() =>
				{
					everyUpdateSubscription.Dispose();
					isStateSubscription.Dispose();
				});
			});
		}
    }
}