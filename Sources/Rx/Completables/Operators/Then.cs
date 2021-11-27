using System;

namespace UniRx.Completables.Operators
{
    internal class ThenCompletable<TSource> : OperatorCompletableBase
    {
        private readonly IObservable<TSource> source;
        private readonly Func<TSource, ICompletable> selectorWithParam;
        private readonly Func<ICompletable> selectorWithoutParam;

        public ThenCompletable(IObservable<TSource> source, Func<TSource, ICompletable> selectorWithParam)
            : base(source.IsRequiredSubscribeOnCurrentThread())
        {
            this.source = source;
            this.selectorWithParam = selectorWithParam;
        }

        public ThenCompletable(IObservable<TSource> source, Func<ICompletable> selectorWithoutParam)
            : base(source.IsRequiredSubscribeOnCurrentThread())
        {
            this.source = source;
            this.selectorWithoutParam = selectorWithoutParam;
        }

        protected override IDisposable SubscribeCore(ICompletableObserver observer, IDisposable cancel)
        {
            return new ThenObserver(this, observer, cancel).Run();
        }

        private class ThenObserver : IObserver<TSource>, IDisposable
        {
            private readonly ThenCompletable<TSource> parent;
            private ICompletableObserver observer;
            private IDisposable cancel;
            private readonly SerialDisposable serialDisposable = new SerialDisposable();

            private bool hasValue;
            private TSource lastValue;

            public ThenObserver(ThenCompletable<TSource> parent, ICompletableObserver observer, IDisposable cancel)
            {
                this.parent = parent;
                this.observer = observer;
                this.cancel = cancel;
            }

            public IDisposable Run()
            {
                serialDisposable.Disposable = new SingleAssignmentDisposable
                {
                    Disposable = parent.source.Subscribe(this)
                };

                return serialDisposable;
            }

            public void OnNext(TSource value)
            {
                hasValue = true;
                lastValue = value;
            }

            public void OnError(Exception error)
            {
                try
                {
                    observer.OnError(error);
                }
                finally
                {
                    Dispose();
                }
            }

            public void OnCompleted()
            {
                if (parent.selectorWithoutParam != null)
                {
                    var v = parent.selectorWithoutParam();
                    serialDisposable.Disposable = v.Subscribe(observer);
                }
                else
                {
                    if (hasValue)
                    {
                        var v = parent.selectorWithParam(lastValue);
                        serialDisposable.Disposable = v.Subscribe(observer);
                    }
                    else
                    {
                        observer.OnError(
                            new InvalidOperationException(
                                "Then() operator never received OnNext() call from parent observable, necessary in order to pass an argument to selector Func<T, ICompletable>"));
                    }
                }
            }

            public void Dispose()
            {
                observer = InternalUtil.EmptyCompletableObserver.Instance;
                var target = System.Threading.Interlocked.Exchange(ref cancel, null);
                target?.Dispose();
            }
        }
    }
}