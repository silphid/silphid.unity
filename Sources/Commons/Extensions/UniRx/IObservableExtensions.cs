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
        public static IObservable<TRet>
            Then<T, TRet>(this IObservable<T> observable, Func<IObservable<TRet>> selector) =>
            observable.AsSingleUnitObservable()
                      .ContinueWith(_ => selector());

        [Pure]
        public static IObservable<TRet> Then<T, TRet>(this IObservable<T> observable, IObservable<TRet> other) =>
            observable.AsSingleUnitObservable()
                      .ContinueWith(_ => other);

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
            This.Where(
                x =>
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
        public static IObservable<TR> SelectNotNull<T, TR>(this IObservable<T> This,
                                                           Func<T, TR> selector,
                                                           Action<T> nullHandler)
        {
            return This.Select(
                            x =>
                            {
                                var result = selector(x);
                                if (result == null)
                                    nullHandler(x);
                                return result;
                            })
                       .WhereNotNull();
        }

        [Pure]
        public static IObservable<T> FirstEmpty<T>(this IObservable<T> source)
        {
            return source.First()
                         .Catch<T, InvalidOperationException>(e => Observable.Empty<T>());
        }
        
        [Pure]
        public static IObservable<T> FirstEmpty<T>(this IObservable<T> source, Func<T, bool> predicate)
        {
            return source.First(predicate)
                         .Catch<T, InvalidOperationException>(e => Observable.Empty<T>());
        }

        [Pure]
        public static ICompletable SelectMany<T>(this IObservable<T> This, Func<T, ICompletable> selector) =>
            This.SelectMany(
                     item => selector(item)
                        .AsEmptyUnitObservable())
                .AsCompletable();

        [Pure]
        public static ICompletable SelectSwitch<T>(this IObservable<T> This, Func<T, ICompletable> selector) =>
            This.Select(
                     item => selector(item)
                        .AsEmptyUnitObservable())
                .Switch()
                .AsCompletable();

        [Pure]
        public static IObservable<TR>
            SelectSwitch<T, TR>(this IObservable<T> This, Func<T, IObservable<TR>> selector) =>
            This.Select(selector)
                .Switch();

        [Pure]
        public static IObservable<bool> FirstTrue(this IObservable<bool> This) =>
            This.First(x => x);

        [Pure]
        public static IObservable<bool> FirstFalse(this IObservable<bool> This) =>
            This.First(x => !x);

        [Pure]
        public static IObservable<Unit> WhereTrue(this IObservable<bool> This) =>
            This.Where(x => x)
                .Select(_ => Unit.Default);

        [Pure]
        public static IObservable<Unit> WhereFalse(this IObservable<bool> This) =>
            This.Where(x => !x)
                .Select(_ => Unit.Default);

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
            This.AutoDetach()
                .Subscribe();

        public static IDisposable SubscribeAndForget<T>(this IObservable<T> This, IObserver<T> observer) =>
            This.AutoDetach()
                .Subscribe(observer);

        public static IDisposable SubscribeAndForget<T>(this IObservable<T> This, Action<T> onNext) =>
            This.AutoDetach()
                .Subscribe(onNext);

        public static IDisposable SubscribeAndForget<T>(this IObservable<T> This,
                                                        Action<T> onNext,
                                                        Action<Exception> onError) =>
            This.AutoDetach()
                .Subscribe(onNext, onError);

        public static IDisposable
            SubscribeAndForget<T>(this IObservable<T> This, Action<T> onNext, Action onCompleted) =>
            This.AutoDetach()
                .Subscribe(onNext, onCompleted);

        public static IDisposable SubscribeAndForget<T>(this IObservable<T> This,
                                                        Action<T> onNext,
                                                        Action<Exception> onError,
                                                        Action onCompleted) =>
            This.AutoDetach()
                .Subscribe(onNext, onError, onCompleted);

        #endregion

        #region Misc

        [Pure]
        public static IObservable<T> MergeErrors<T, TException>(this IObservable<T> This,
                                                                IObservable<TException> errors)
            where TException : Exception =>
            This.Merge(errors.SelectMany(ex => Observable.Throw<T>(ex)));

        [Pure]
        public static IObservable<T> DoOnError<T, TException>(this IObservable<T> This, Action<TException> action)
            where TException : Exception =>
            This.DoOnError(
                ex =>
                {
                    var t = ex as TException;
                    if (t != null)
                        action(t);
                });

        [Pure]
        public static IObservable<T> DoOnError<T, TException>(this IObservable<T> This, Action action)
            where TException : Exception =>
            This.DoOnError(
                ex =>
                {
                    if (ex is TException)
                        action();
                });

        [Pure]
        public static IObservable<T> First<T>(this IObservable<T> This, T value) =>
            This.First(x => Equals(x, value));

        [Pure]
        public static IObservable<T> FirstOrDefault<T>(this IObservable<T> This, T value) =>
            This.FirstOrDefault(x => Equals(x, value));

        [Pure]
        public static IObservable<T> Where<T>(this IObservable<T> This, T value) =>
            This.Where(x => Equals(x, value));

        [Pure]
        public static IObservable<Tuple<TSource, TSource>>
            PairWithPrevious<TSource>(this IObservable<TSource> source) =>
            source.Scan(
                       Tuple.Create(default(TSource), default(TSource)),
                       (acc, current) => Tuple.Create(acc.Item2, current))
                  .Skip(1);

        [Pure]
        public static IObservable<Tuple<TSource, TSource>> PairWithPreviousOrDefault<TSource>(
            this IObservable<TSource> source) =>
            source.Scan(
                Tuple.Create(default(TSource), default(TSource)),
                (acc, current) => Tuple.Create(acc.Item2, current));

        public static IDisposable SubscribeCompletion<T>(this IObservable<T> This, Action onCompleted) =>
            This.AutoDetach()
                .Subscribe(Observer.Create<T>(_ => {}, ex => { throw ex; }, onCompleted));

        /// <summary>
        /// Waits given delay before emitting each item it receives and cancels that emitting if another item
        /// is received in the meantime. Very useful for only updating UI when value is stable enough.
        /// </summary>
        public static IObservable<T> LazyThrottle<T>(this IObservable<T> This, TimeSpan delay) =>
            Observable.Create<T>(
                observer =>
                {
                    var serialDisposable = new SerialDisposable();

                    return new CompositeDisposable(
                        serialDisposable,
                        This.Subscribe(
                            x => serialDisposable.Disposable = Observable.Timer(delay)
                                                                         .Subscribe(_ => observer.OnNext(x)),
                            observer.OnCompleted));
                });

        public static IObservable<T> Debug<T>(this IObservable<T> This, Func<T, string> formatter) =>
#if DEBUG
            This.Do(x => UnityEngine.Debug.Log(formatter(x)));
#else
            This;
#endif

        public static IObservable<TSource> CatchIgnore<TSource, TException>(this IObservable<TSource> This)
            where TException : Exception =>
            This.CatchIgnore<TSource, TException>(_ => {});

        #endregion

        #region Repeat

        private static IEnumerable<IObservable<T>> RepeatInternal<T>(this IObservable<T> This, int count)
        {
            for (var i = 0; i < count; i++)
                yield return This;
        }

        public static IObservable<T> Repeat<T>(this IObservable<T> This, int count) =>
            This.RepeatInternal(count)
                .Concat();

        #endregion

        #region Prepend/Append

        public static IObservable<T> Prepend<T>(this IObservable<T> This, T item) =>
            Observable.Return(item)
                      .Concat(This);

        public static IObservable<T> Append<T>(this IObservable<T> This, T item) =>
            This.Concat(Observable.Return(item));

        #endregion

        #region Binding

        public static IDisposable BindTo<TSource, TTarget>(this IObservable<TSource> This,
                                                           IReactiveProperty<TTarget> target) where TTarget : TSource =>
            This.Subscribe(x => target.Value = (TTarget) x);

        #endregion

        #region QueuedSelectMany

        private class QueuedSelectManyOperator<T, TR>
        {
            private readonly IObservable<T> _observable;
            private readonly Func<T, IObservable<TR>> _selector;
            private readonly Queue<T> _queue = new Queue<T>();
            private readonly Subject<TR> _subject = new Subject<TR>();
            private bool _isProcessing;

            public QueuedSelectManyOperator(IObservable<T> observable, Func<T, IObservable<TR>> selector)
            {
                _observable = observable;
                _selector = selector;
            }

            public IObservable<TR> Run()
            {
                return _subject.DoOnSubscribe(
                    () =>
                    {
                        _observable.Subscribe(
                            input =>
                            {
                                bool shouldProcessNow = false;

                                lock (this)
                                {
                                    if (_isProcessing)
                                        _queue.Enqueue(input);
                                    else
                                    {
                                        _isProcessing = true;
                                        shouldProcessNow = true;
                                    }
                                }

                                if (shouldProcessNow)
                                    Process(input);
                            },
                            () =>
                            {
                                bool shouldCompleteNow;

                                lock (this)
                                {
                                    shouldCompleteNow = !_isProcessing;
                                }

                                if (shouldCompleteNow)
                                    _subject.OnCompleted();
                            });
                    });
            }

            private void Process(T input)
            {
                _selector(input)
                   .Subscribe(
                        output => _subject.OnNext(output),
                        () =>
                        {
                            // Upon completion check if more items have been enqueued in the mean time

                            var nextInput = default(T);
                            bool shouldProcessNow = false;

                            lock (this)
                            {
                                if (_queue.Count > 0)
                                {
                                    nextInput = _queue.Dequeue();
                                    shouldProcessNow = true;
                                }
                                else
                                    _isProcessing = false;
                            }

                            if (shouldProcessNow)
                                Process(nextInput);
                        });
            }
        }

        public static IObservable<TR> QueuedSelectMany<T, TR>(this IObservable<T> observable,
                                                              Func<T, IObservable<TR>> selector) =>
            new QueuedSelectManyOperator<T, TR>(observable, selector).Run();

        #endregion

        #region Reactive properties

        public static ReactiveProperty<T> ToWritableReactiveProperty<T>(this IObservable<T> source)
        {
            var property = new ReactiveProperty<T>();
            source.BindTo(property);
            return property;
        }

        public static ReactiveProperty<T> ToWritableReactiveProperty<T>(this IObservable<T> source, T initialValue)
        {
            var property = new ReactiveProperty<T>(initialValue);
            source.BindTo(property);
            return property;
        }

        #endregion

        #region Serial

        public static IObservable<T> DoSerialDispose<T>(this IObservable<T> This, Func<T, IDisposable> action)
        {
            var serial = new SerialDisposable();
            return This.Do(x => serial.Disposable = action(x));
        }

        public static IObservable<bool> DoSerialDisposeOnTrue(this IObservable<bool> This, Func<IDisposable> action) =>
            This.DoSerialDispose(
                x => x
                         ? action()
                         : null);

        #endregion

        public static IObservable<ValueTuple<TLeft, TRight>> CombineLatest<TLeft, TRight>(
            this IObservable<TLeft> left,
            IObservable<TRight> right) =>
            left.CombineLatest(right, (x, y) => (x, y));

        public static IObservable<ValueTuple<T1, T2, T3>> CombineLatest<T1, T2, T3>(
            this IObservable<T1> o1,
            IObservable<T2> o2,
            IObservable<T3> o3) =>
            o1.CombineLatest(o2, o3, (x, y, z) => (x, y, z));
    }
}