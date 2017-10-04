using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UniRx.Operators;
using UniRx;

namespace Silphid.Extensions
{
    public static class IObservableExtensions
    {
        #region Then and ThenReturn

        [Pure]
        public static IObservable<TRet> Then<T, TRet>(this IObservable<T> observable, Func<IObservable<TRet>> selector) =>
            observable.AsSingleUnitObservable().ContinueWith(_ => selector());

        [Pure]
        public static IObservable<TRet> Then<T, TRet>(this IObservable<T> observable, IObservable<TRet> other) =>
            observable.AsSingleUnitObservable().ContinueWith(_ => other);

        [Pure]
        public static IObservable<TRet> ThenReturn<T, TRet>(this IObservable<T> observable, TRet value) =>
            observable.Then(() => Observable.Return(value));

        #endregion

        #region AutoDetach

        internal class AutoDetachObservable<T> : OperatorObservableBase<T>
        {
            private readonly IObservable<T> _source;

            public AutoDetachObservable(IObservable<T> source)
                : base(source.IsRequiredSubscribeOnCurrentThread())
            {
                _source = source;
            }

            protected override IDisposable SubscribeCore(IObserver<T> observer, IDisposable cancel) =>
                _source.Subscribe(Observer.CreateAutoDetachObserver(observer, cancel));
        }

        [Pure]
        public static IObservable<T> AutoDetach<T>(this IObservable<T> This) =>
            new AutoDetachObservable<T>(This);

        #endregion

        #region Nulls and booleans

        [Pure]
        public static IObservable<T> WhereNotNull<T>(this IObservable<T> This) =>
            This.Where(x => x != null);

        [Pure]
        public static IObservable<T> WhereNotNull<T>(this IObservable<T> This, Action nullHandler) =>
            This.Where(x =>
            {
                if (x == null)
                {
                    nullHandler();
                    return false;
                }                
                return true;
            });

        [Pure]
        public static IObservable<TR> SelectNotNull<T, TR>(this IObservable<T> This, Func<T, TR> selector) =>
            This.Select(selector)
                .WhereNotNull();

        [Pure]
        public static IObservable<TR> SelectNotNull<T, TR>(this IObservable<T> This, Func<T, TR> selector, Action<T> nullHandler)
        {
            return This.Select(x =>
                {
                    var result = selector(x);
                    if (result == null)
                        nullHandler(x);
                    return result;
                })
                .WhereNotNull();
        }

        [Pure]
        public static IObservable<Unit> WhereTrue(this IObservable<bool> This) =>
            This.Where(x => x).Select(_ => Unit.Default);

        [Pure]
        public static IObservable<Unit> WhereFalse(this IObservable<bool> This) =>
            This.Where(x => !x).Select(_ => Unit.Default);

        [Pure]
        public static IObservable<bool> Not(this IObservable<bool> This) =>
            This.Select(x => !x);

        [Pure]
        public static IObservable<bool> And(this IObservable<bool> This, IObservable<bool> other) =>
            This.CombineLatest(other, (x, y) => x && y);

        [Pure]
        public static IObservable<bool> Or(this IObservable<bool> This, IObservable<bool> other) =>
            This.CombineLatest(other, (x, y) => x || y);

        #endregion

        #region SubscribeAndForget

        public static IDisposable SubscribeAndForget<T>(this IObservable<T> This) =>
            This.AutoDetach().Subscribe();

        public static IDisposable SubscribeAndForget<T>(this IObservable<T> This, IObserver<T> observer) =>
            This.AutoDetach().Subscribe(observer);

        public static IDisposable SubscribeAndForget<T>(this IObservable<T> This, Action<T> onNext) =>
            This.AutoDetach().Subscribe(onNext);

        public static IDisposable SubscribeAndForget<T>(this IObservable<T> This, Action<T> onNext, Action<Exception> onError) =>
            This.AutoDetach().Subscribe(onNext, onError);

        public static IDisposable SubscribeAndForget<T>(this IObservable<T> This, Action<T> onNext, Action onCompleted) =>
            This.AutoDetach().Subscribe(onNext, onCompleted);

        public static IDisposable SubscribeAndForget<T>(this IObservable<T> This, Action<T> onNext, Action<Exception> onError, Action onCompleted) =>
            This.AutoDetach().Subscribe(onNext, onError, onCompleted);

        #endregion

        #region Misc

        [Pure]
        public static IObservable<Tuple<TSource, TSource>> PairWithPrevious<TSource>(this IObservable<TSource> source) =>
            source
                .Scan(Tuple.Create(default(TSource), default(TSource)), (acc, current) => Tuple.Create(acc.Item2, current))
                .Skip(1);
        
        [Pure]
        public static IObservable<Tuple<TSource, TSource>> PairWithPreviousOrDefault<TSource>(this IObservable<TSource> source) =>
            source.Scan(Tuple.Create(default(TSource), default(TSource)), (acc, current) => Tuple.Create(acc.Item2, current));

        public static IDisposable SubscribeCompletion<T>(this IObservable<T> This, Action onCompleted) =>
            This.AutoDetach().Subscribe(Observer.Create<T>(_ => {}, ex => { throw ex; }, onCompleted));

        /// <summary>
        /// Waits given delay before emitting each item it receives and cancels that emitting if another item
        /// is received in the meantime. Very useful for only updating UI when value is stable enough.
        /// </summary>
        public static IObservable<T> LazyThrottle<T>(this IObservable<T> This, TimeSpan delay) =>
            Observable.Create<T>(observer =>
            {
                var serialDisposable = new SerialDisposable();

                return new CompositeDisposable(
                    serialDisposable,
                    This
                        .Subscribe(x => serialDisposable.Disposable = Observable
                            .Timer(delay)
                            .Subscribe(_ => observer.OnNext(x)), observer.OnCompleted));
            });

        public static IObservable<T> Debug<T>(this IObservable<T> This, Func<T, string> formatter) =>
#if DEBUG
            This.Do(x => UnityEngine.Debug.Log(formatter(x)));
#else
            This;
#endif

        #endregion

        #region Repeat

        private static IEnumerable<IObservable<T>> RepeatInternal<T>(this IObservable<T> This, int count)
        {
            for (var i = 0; i < count; i++)
                yield return This;
        }

        public static IObservable<T> Repeat<T>(this IObservable<T> This, int count) =>
            This.RepeatInternal(count).Concat();
        
        #endregion

        #region Prepend/Append

        public static IObservable<T> Prepend<T>(this IObservable<T> This, T item) =>
            Observable.Return(item).Concat(This);

        public static IObservable<T> Append<T>(this IObservable<T> This, T item) =>
            This.Concat(Observable.Return(item));

        #endregion
    }
}