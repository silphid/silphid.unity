using System;
using Silphid.Extensions;
using UniRx;

namespace Silphid.Machina
{
    public static class IMachineExtensions
    {
	    public static IObservable<Change<TState>> Changes<TState>(this IObservable<TState> This) where TState : IState =>
		    This.PairWithPrevious().Select(x => new Change<TState>(x.Item1, x.Item2));

	    public static IObservable<Change<TState>> Changes<TState>(this IMachine<TState> This) where TState : IState =>
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

		public static IObservable<Change<TState>> Exiting<TState>(this IObservable<Change<TState>> This, TState state) where TState : IState
        {
			return This.Where(x => x.Source.Is(state));
        }

		public static IObservable<Change<TState>> Entering<TState>(this IObservable<Change<TState>> This, TState state) where TState : IState
        {
			return This.Where(x => x.Destination.Is(state));
        }

		public static IObservable<Change<TState>> Changing<TState>(this IObservable<Change<TState>> This, TState source, TState destination) where TState : IState
        {
			return This.Exiting(source).Entering(destination);
        }

		public static IObservable<Change<TState>> Exiting<TState>(this IMachine<TState> This, TState state) where TState : IState
        {
            return This.Changes().Exiting(state);
        }

		public static IObservable<Change<TState>> Entering<TState>(this IMachine<TState> This, TState state) where TState : IState
        {
            return This.Changes().Entering(state);
        }

		public static IObservable<Change<TState>> Changing<TState>(this IMachine<TState> This, TState source, TState destination) where TState : IState
        {
			return This.Changes().Changing(source, destination);
        }

		public static IObservable<bool> Is<TState>(this IMachine<TState> This, TState state) where TState : IState
        {
            return This.State.Select(x => x.Is(state)).DistinctUntilChanged();
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