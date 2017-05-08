using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UniRx.Operators;
using Rx = UniRx;
using UniRx;

namespace Silphid.Extensions
{
    public static class IObservableExtensions
    {
        #region Then and ThenReturn

        [Pure]
        public static Rx.IObservable<TRet> Then<T, TRet>(this Rx.IObservable<T> observable, Func<Rx.IObservable<TRet>> selector) =>
            observable.AsSingleUnitObservable().ContinueWith(_ => selector());

        [Pure]
        public static Rx.IObservable<TRet> Then<T, TRet>(this Rx.IObservable<T> observable, Rx.IObservable<TRet> other) =>
            observable.AsSingleUnitObservable().ContinueWith(_ => other);

        [Pure]
        public static Rx.IObservable<TRet> ThenReturn<T, TRet>(this Rx.IObservable<T> observable, TRet value) =>
            observable.Then(() => Observable.Return(value));

        #endregion

        #region AutoDetach

        internal class AutoDetachObservable<T> : OperatorObservableBase<T>
        {
            private readonly Rx.IObservable<T> _source;

            public AutoDetachObservable(Rx.IObservable<T> source)
                : base(source.IsRequiredSubscribeOnCurrentThread())
            {
                _source = source;
            }

            protected override IDisposable SubscribeCore(Rx.IObserver<T> observer, IDisposable cancel) =>
                _source.Subscribe(Observer.CreateAutoDetachObserver(observer, cancel));
        }

        [Pure]
        public static Rx.IObservable<T> AutoDetach<T>(this Rx.IObservable<T> This) =>
            new AutoDetachObservable<T>(This);

        #endregion

        #region Nulls and booleans

        [Pure]
        public static Rx.IObservable<T> WhereNotNull<T>(this Rx.IObservable<T> This) =>
            This.Where(x => x != null);

        [Pure]
        public static Rx.IObservable<Unit> WhereTrue(this Rx.IObservable<bool> This) =>
            This.Where(x => x).Select(_ => Unit.Default);

        [Pure]
        public static Rx.IObservable<Unit> WhereFalse(this Rx.IObservable<bool> This) =>
            This.Where(x => !x).Select(_ => Unit.Default);

        [Pure]
        public static Rx.IObservable<bool> Negate(this Rx.IObservable<bool> This) =>
            This.Select(x => !x);

        #endregion

        #region SubscribeAndForget

        public static void SubscribeAndForget<T>(this Rx.IObservable<T> This)
        {
            This.AutoDetach().Subscribe();
        }

        public static void SubscribeAndForget<T>(this Rx.IObservable<T> This, Rx.IObserver<T> observer)
        {
            This.AutoDetach().Subscribe(observer);
        }

        public static void SubscribeAndForget<T>(this Rx.IObservable<T> This, Action<T> onNext)
        {
            This.AutoDetach().Subscribe(onNext);
        }

        public static void SubscribeAndForget<T>(this Rx.IObservable<T> This, Action<T> onNext, Action<Exception> onError)
        {
            This.AutoDetach().Subscribe(onNext, onError);
        }

        public static void SubscribeAndForget<T>(this Rx.IObservable<T> This, Action<T> onNext, Action onCompleted)
        {
            This.AutoDetach().Subscribe(onNext, onCompleted);
        }

        public static void SubscribeAndForget<T>(this Rx.IObservable<T> This, Action<T> onNext, Action<Exception> onError, Action onCompleted)
        {
            This.AutoDetach().Subscribe(onNext, onError, onCompleted);
        }

        #endregion

        #region Misc

        [Pure]
        public static Rx.IObservable<Rx.Tuple<TSource, TSource>> PairWithPrevious<TSource>(this Rx.IObservable<TSource> source)
            =>
            source.Scan(
                Rx.Tuple.Create(default(TSource), default(TSource)),
                (acc, current) => Rx.Tuple.Create(acc.Item2, current));

        public static IDisposable SubscribeCompletion<T>(this Rx.IObservable<T> This, Action onCompleted)
        {
            return This.AutoDetach().Subscribe(Observer.Create<T>(_ => { }, ex => { throw ex; }, onCompleted));
        }

        #endregion

        #region Repeat

        private static IEnumerable<Rx.IObservable<T>> RepeatInternal<T>(this Rx.IObservable<T> This, int count)
        {
            for (var i = 0; i < count; i++)
                yield return This;
        }

        public static Rx.IObservable<T> Repeat<T>(this Rx.IObservable<T> This, int count) =>
            This.RepeatInternal(count).Concat();
        
        #endregion
    }
}