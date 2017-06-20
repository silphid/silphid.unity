using System;
using UniRx;
using Action = System.Action;

namespace Silphid.Sequencit.Machines
{
    public static class IMachineExtensions
    {
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

		public static IObservable<Change<TState>> OnExit<TState>(this IObservable<Change<TState>> This, TState state) where TState : IState
        {
			return This.Where(x => x.Source.Is(state));
        }

		public static IObservable<Change<TState>> OnEnter<TState>(this IObservable<Change<TState>> This, TState state) where TState : IState
        {
			return This.Where(x => x.Destination.Is(state));
        }

		public static IObservable<Change<TState>> OnChange<TState>(this IObservable<Change<TState>> This, TState source, TState destination) where TState : IState
        {
			return This.OnExit(source).OnEnter(destination);
        }

		public static IObservable<Change<TState>> OnExit<TState>(this IMachine<TState> This, TState state) where TState : IState
        {
            return This.Changes.OnExit(state);
        }

		public static IObservable<Change<TState>> OnEnter<TState>(this IMachine<TState> This, TState state) where TState : IState
        {
            return This.Changes.OnEnter(state);
        }

		public static void OnChange<TState>(this MachineBehaviour<TState> This, TState source, TState destination, Action action) where TState : class, IState
		{
		    This.OnExit(source).OnEnter(destination).Subscribe(_ => action()).AddTo(This);
		}

		public static void OnExit<TState>(this MachineBehaviour<TState> This, TState state, Action action) where TState : class, IState
		{
		    This.OnExit(state).Subscribe(_ => action()).AddTo(This);
		}

		public static void OnEnter<TState>(this MachineBehaviour<TState> This, TState state, Action action) where TState : class, IState
        {
            This.OnEnter(state).Subscribe(_ => action()).AddTo(This);
        }

		public static IObservable<Change<TState>> OnChange<TState>(this IMachine<TState> This, TState source, TState destination) where TState : IState
        {
			return This.Changes.OnChange(source, destination);
        }

		public static IObservable<bool> OnState<TState>(this IMachine<TState> This, TState state) where TState : IState
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